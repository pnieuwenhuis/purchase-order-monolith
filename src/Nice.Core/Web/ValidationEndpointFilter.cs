using System.Reflection;
using Microsoft.AspNetCore.Http;
using Nice.Core.Validation;

namespace Nice.Core.Web;

public static class ValidationFilter
{
    public static EndpointFilterDelegate ValidationFilterFactory(EndpointFilterFactoryContext context, EndpointFilterDelegate next) =>
        invocationContext => Validate(context.MethodInfo, invocationContext, next);

    private static async ValueTask<object?> Validate(MethodInfo methodInfo, EndpointFilterInvocationContext invocationContext, EndpointFilterDelegate next)
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            var argument = invocationContext.Arguments[i];
            if (argument is not null)
            {
                var type = argument.GetType();
                if (await ValidationHelper.PerformRequestValidation(argument, type) is IResult result)
                {
                    return result;
                }
            }
        }

        return await next.Invoke(invocationContext);
    }
}
