using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Nice.Core.Definitions;

namespace Nice.Core;

public static partial class Service
{
    public static IServiceCollection AddSwagger(this IServiceCollection services, ApiServiceResourceDefinition apiService) =>
            services.AddSwaggerGen(c =>
            {
                apiService.Metadata.Annotations.TryGetValue("owner", out var ownerValue);
                c.SwaggerDoc(
                    apiService.Metadata.Version,
                    new OpenApiInfo
                    {
                        Title = $"{apiService.Metadata.Name} API",
                        Description = apiService.Metadata.Description,
                        Contact = new OpenApiContact { Name = ownerValue ?? string.Empty },
                        Version = apiService.Metadata.Version
                    });
                c.OrderActionsBy(x => $"{x.HttpMethod}_{x.RelativePath}");

                c.AddSecurityDefinition("Custom Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "JWT Authorization",
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });
                c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
                {
                    Description = "Basic authorization",
                    Name = "Basic Authorization",
                    Scheme = "basic",
                    Type = SecuritySchemeType.Http,
                });
                c.CustomSchemaIds(type => type.FullName?.Replace("+", "_", true, CultureInfo.InvariantCulture));
            });
}
