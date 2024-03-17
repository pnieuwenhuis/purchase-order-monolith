global using System.Text.Json;
global using System.Text.Json.Serialization;
global using Injectio.Attributes;
global using Nice.Core;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.Extensions.DependencyInjection;

using Nice.PurchaseOrder.Domain.Data;
using Nice.PurchaseOrder.Domain.Endpoints;

[module: Dapper.DapperAot]

namespace Nice.PurchaseOrder.Domain;

public class ServiceModule : IAppModule
{
    public void Configure(ModuleConfigurator configurator)
    {
        configurator.RegisterAspNetJsonSerializerContext(PublicEndpointsJsonSerializerContext.Default);
        configurator.RegisterAspNetJsonSerializerContext(DataJsonSerializerContext.Default);
        configurator.Register(services => services.AddDomainServices());
    }
}
