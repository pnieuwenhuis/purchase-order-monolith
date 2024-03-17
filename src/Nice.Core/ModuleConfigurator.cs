using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nice.Core.Exceptions;
using Nice.Core.Validation;

namespace Nice.Core;

public class ModuleConfigurator(IServiceCollection services, IConfiguration configuration, IHealthChecksBuilder healthChecksBuilder)
{
    internal IList<JsonSerializerContext> JsonSerializerContexts { get; } = [];
    internal IList<Type> Initializers { get; } = [];

    public void RegisterSettingsWithValidator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TSettings, TValidator>(string configurationSection, Func<IConfigurationSection, TSettings?> configure)
        where TSettings : class, ISettings
        where TValidator : IValidator<TSettings>, new()
    {
        var settings = configure(configuration.GetSection(configurationSection));
        if (settings is not null)
        {
            ValidationHelper.PerformSettingsValidation(settings);
            services.AddSingleton(settings);
        }
        else throw new ServiceException($"Could not load configuration section '{configurationSection}'");
    }

    public void Register(Action<IServiceCollection> configure) => configure(services);

    public void RegisterAspNetJsonSerializerContext(JsonSerializerContext context) => JsonSerializerContexts.Add(context);

    public void SetInitializer<TInitializer>() where TInitializer : IModuleInitializer => Initializers.Add(typeof(TInitializer));

    public void RegisterHealhCheck(Action<IHealthChecksBuilder> configure) => configure(healthChecksBuilder);
}
