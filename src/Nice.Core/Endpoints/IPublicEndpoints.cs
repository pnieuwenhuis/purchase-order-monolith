using Microsoft.AspNetCore.Routing;

namespace Nice.Core.Endpoints;

public interface IPublicEndpoints
{
    string RoutePrefix { get; }
    void RegisterEndpoints(RouteGroupBuilder builder);
}
