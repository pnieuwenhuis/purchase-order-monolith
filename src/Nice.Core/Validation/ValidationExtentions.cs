using Microsoft.AspNetCore.Http;

namespace Nice.Core.Validation
{
    public static class ValidationExtentions
    {
        public static IResult ToValidationProblem(this (string Code, string Description) tuple) =>
            Results.ValidationProblem(new Dictionary<string, string[]> { { tuple.Code, new[] { tuple.Description } } });
    }
}