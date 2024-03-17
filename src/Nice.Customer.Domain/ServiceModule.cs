﻿global using System.Text.Json;
global using System.Text.Json.Serialization;
global using Injectio.Attributes;
global using Nice.Core;
global using Nice.Core.Exceptions;
global using Nice.Core.Endpoints;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.Extensions.DependencyInjection;

using Nice.Customer.Domain.Endpoints;
using Nice.Customer.Domain.Data;

[module: Dapper.DapperAot]

namespace Nice.Customer.Domain;

public class ServiceModule : IAppModule
{
    public void Configure(ModuleConfigurator configurator)
    {
        configurator.RegisterAspNetJsonSerializerContext(PublicEndpointsJsonSerializerContext.Default);
        configurator.RegisterAspNetJsonSerializerContext(DataJsonSerializerContext.Default);
        configurator.Register(services => services.AddDomainServices());
    }
}
