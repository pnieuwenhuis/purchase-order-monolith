using System.Globalization;
using System.Text.Json;
using DotNetEnv.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nice.Core.Definitions;
using Nice.Core.Endpoints;
using Nice.Core.Web;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Nice.Core;

public static partial class Service
{
    private const string MetadataYamlFile = "nice.yaml";

    public static void Start(IEnumerable<IAppModule> appModules)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) => Log.Fatal("Fatal Error", args.ExceptionObject as Exception);
        FluentValidation.ValidatorOptions.Global.DisplayNameResolver = (type, member, expr) => member.Name; // Global FluentValidation options

        var builder = WebApplication.CreateSlimBuilder();

        Log.Logger = new LoggerConfiguration()
            .ServiceLoggingConfiguration()
            .Enrich.WithProperty("SourceContext", "Startup")
            .CreateBootstrapLogger();

        DirectoryInfo applicationRoot = GetRootPath(new DirectoryInfo(Directory.GetCurrentDirectory())) ??
            throw new InvalidOperationException($"Could not find {MetadataYamlFile} in current directory or one of the parent directories"); ;
        Log.Information($"Found root path of this service at: {applicationRoot}");
        ApiServiceResourceDefinition apiServiceDefinition = LoadApiServiceDefinition(applicationRoot);
        Log.Information($"Starting API service with name '{apiServiceDefinition.Metadata.Name}'. Will listen on HTTP port {apiServiceDefinition.Spec.Ports["http"]}");
        IConfigurationRoot configuration = LoadConfiguration(applicationRoot);

        // ** Logging configuration
        builder.Host.UseSerilog((ctx, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ServiceLoggingConfiguration());
        if (!IsProductionEnvironment())
        {
            // Configure full HTTP logging in non-production environments
            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
                logging.RequestHeaders.Add("Origin");
                logging.RequestHeaders.Add("Referer");
                logging.ResponseHeaders.Add("Content-Encoding");
                logging.RequestBodyLogLimit = 8192;
                logging.ResponseBodyLogLimit = 8192;
            });
        }

        // ** HTTP Server configuration
        builder.WebHost
            .UseKestrel()
            .UseUrls($"http://*:{apiServiceDefinition.Spec.Ports["http"]}")
            .SuppressStatusMessages(true)
            .UseConfiguration(configuration)
            .ConfigureLogging((env, logging) => logging.ClearProviders());

        // ** Services
        builder.Services
            .AddSingleton<IClock, SystemClock>()
            .AddResponseCompression(options => options.Providers.Add<GzipCompressionProvider>())
            .AddMemoryCache()
            .AddEndpointsApiExplorer()
            .AddSwagger(apiServiceDefinition)
            .AddProblemDetails()
            .AddOpenTelemetry()
                .WithMetrics(builder =>
                    {
                        builder.AddPrometheusExporter();

                        builder.AddMeter("Microsoft.AspNetCore.Hosting",
                                         "Microsoft.AspNetCore.Server.Kestrel");
                        builder.AddView("http.server.request.duration",
                            new ExplicitBucketHistogramConfiguration
                            {
                                Boundaries = [ 0, 0.005, 0.01, 0.025, 0.05,
                                       0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 ]
                            });
                    });

        // ** Loop though all module configurators and add module specific services to the DI container
        var healthCheckBuilder = builder.Services.AddHealthChecks();
        var configurator = new ModuleConfigurator(builder.Services, configuration, healthCheckBuilder);
        foreach (IAppModule module in appModules)
        {
            module.Configure(configurator);
        }

        // ** Configure JSON serialization
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, ResponseModelsSerializerContext.Default);
            foreach (var context in configurator.JsonSerializerContexts)
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, context);
            }
        });

        var app = builder.Build();

        // Run all module initializers asynconously
        IEnumerable<bool> initTasks = Task.WhenAll(
            configurator.Initializers.Select(initializer => ((IModuleInitializer)app.Services.GetRequiredService(initializer)).Run())).GetAwaiter().GetResult();
        if (initTasks.Any(x => !x))
        {
            Log.Fatal("Could not initialize service because one or more initializers failed!");
            Environment.Exit(1);
        }

        ConfigureInternalEndpoints(app);
        ConfigurePublicEndpoints(app);
        app.Run();
    }

    private static void ConfigureInternalEndpoints(this WebApplication app)
    {
        var openApiBasePath = "internal/openapi";
        app.UseSwagger(c => c.RouteTemplate = $"{openApiBasePath}/{{documentName}}/definition.{{json|yaml}}");
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerUI(o => o.RoutePrefix = openApiBasePath);
        }

        app.MapPrometheusScrapingEndpoint("/internal/metrics");

        // ** Register internal endpoints
        RouteGroupBuilder internalApiGroup = app.MapGroup("/internal");
        internalApiGroup.MapHealthChecks("/readyz")
            .WithOpenApi()
            .WithDescription("This endpoint is used by Kubernetes to determine if the service is ready to receive traffic")
            .WithDisplayName("Ready Check Endpoint")
            .WithTags("Health Checks");
        internalApiGroup.MapGet("/livez", () => Results.Ok())
            .WithOpenApi()
            .WithDescription("This endpoint is used by Kubernetes to determine if the service is still alive")
            .WithDisplayName("Liveness Check Endpoint")
            .WithTags("Health Checks");
        foreach (var e in app.Services.GetServices<IInternalEndpoints>())
        {
            e.RegisterEndpoints(internalApiGroup);
        }
    }

    private static void ConfigurePublicEndpoints(this WebApplication app)
    {
        RouteGroupBuilder publicApiGroup = app.MapGroup("/public");
        publicApiGroup.AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory);
        foreach (var e in app.Services.GetServices<IPublicEndpoints>())
        {
            RouteGroupBuilder routeBuilder = publicApiGroup.MapGroup(e.RoutePrefix);
            e.RegisterEndpoints(routeBuilder);
        }
    }

    private static DirectoryInfo? GetRootPath(DirectoryInfo? path)
    {
        if (path == null)
        {
            return null;
        }

        if (File.Exists(Path.Combine(path.ToString(), MetadataYamlFile)))
        {
            return path;
        }

        return GetRootPath(path.Parent);
    }

    private static ApiServiceResourceDefinition LoadApiServiceDefinition(DirectoryInfo appRoot)
    {
        var deserializer = new StaticDeserializerBuilder(new YamlStaticContext())
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        try
        {
            using var reader = new StreamReader(Path.Combine(appRoot.ToString(), MetadataYamlFile));
            var yaml = new YamlStream();
            yaml.Load(reader);

            if (!(yaml.Documents[0].RootNode is YamlMappingNode mappingNode)) // Cannot search non mapping nodes
            {
                throw new InvalidOperationException("Could not determine root node in yaml file");
            }

            string apiVersion = mappingNode.Children[new YamlScalarNode("apiVersion")].ToString();
            string kind = mappingNode.Children[new YamlScalarNode("kind")].ToString();
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            switch (kind)
            {
                case "ApiService":
                    return apiVersion switch
                    {
                        "nice.io/v1beta1" => deserializer.Deserialize<ApiServiceResourceDefinition>(reader),
                        _ => throw new InvalidOperationException($"Unsupported apiVersion '{apiVersion}' for resource '{kind}' in {MetadataYamlFile}")
                    };

                default:
                    throw new InvalidOperationException($"Unsupported resource '{kind}' in {MetadataYamlFile}");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not load {MetadataYamlFile} from {appRoot}", ex);
        }
    }

    private static IConfigurationRoot LoadConfiguration(DirectoryInfo appRoot)
    {
        IConfigurationBuilder configBuilder = new ConfigurationBuilder()
            .SetBasePath(appRoot.ToString())
            .AddEnvironmentVariables();

        var envPath = new FileInfo(Path.Combine(appRoot.ToString(), ".env"));
        if (envPath.Exists && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "production")
        {
            Log.Information($"Found .env file at '{envPath}'!");
            configBuilder.AddDotNetEnv(envPath.FullName, DotNetEnv.LoadOptions.NoClobber());
        }

        return configBuilder.Build();
    }

    private static LoggerConfiguration ServiceLoggingConfiguration(this LoggerConfiguration lc) => lc
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
            formatProvider: CultureInfo.InvariantCulture,
            theme: IsProductionEnvironment() ? ConsoleTheme.None : AnsiConsoleTheme.Sixteen);

    public static bool IsProductionEnvironment() => (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development").Equals("production", StringComparison.InvariantCultureIgnoreCase);
}
