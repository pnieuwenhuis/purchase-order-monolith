using Microsoft.AspNetCore.Routing;

namespace Nice.Core.Endpoints;

public interface IInternalEndpoints
{
    void RegisterEndpoints(RouteGroupBuilder builder);
}
