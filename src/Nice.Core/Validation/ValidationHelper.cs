using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Nice.Core.Validation;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public abstract class ValidateAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type validator) : Attribute
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type Validator { get => validator; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ValidateSettingsAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type validator) : ValidateAttribute(validator);

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ValidateRequestAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type validator) : ValidateAttribute(validator);

internal static class ValidationHelper
{
    internal async static ValueTask<IResult?> PerformRequestValidation(object request, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
    {
        IValidator? validator = GetValidator<ValidateRequestAttribute>(type);

        if (validator is not null)
        {
            var validationResult = await validator.ValidateAsync(new ValidationContext<object>(request));
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
        }

        return null;
    }

    internal static void PerformSettingsValidation<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TSettings>(TSettings settings)
        where TSettings : class, ISettings
    {
        IValidator? validator = GetValidator<ValidateSettingsAttribute>(typeof(TSettings));

        if (validator is not null)
        {
            var validationResult = validator.Validate(new ValidationContext<TSettings>(settings));
            if (!validationResult.IsValid)
            {
                throw new Exceptions.ValidationException($"Configuration section '{settings.GetType().Name}' has some validation errors", validationResult.Errors.Select(x => $" - {x.ErrorMessage}"));
            }
        }
    }

    private static IValidator? GetValidator<TAttribute>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
        where TAttribute : ValidateAttribute
    {
        var att = type.GetCustomAttribute<TAttribute>();
        if (att is not null)
        {
            return Activator.CreateInstance(att.Validator) as IValidator;
        }

        return null;
    }
}
