using FluentValidation;
using Nice.Core.Validation;

namespace Nice.Product.Domain.Endpoints;

public record GetProductResponse(int Id, string ShortName, string Description, IDictionary<string, string> Properties, int Price);

[ValidateRequest(typeof(PostProductRequestValidator))]
public record PostProductRequest(
    int? Id,
    string ShortName,
    string Description,
    IDictionary<string, string> Properties,
    int Price);

public class PostProductRequestValidator : AbstractValidator<PostProductRequest>
{
    public PostProductRequestValidator()
    {
        RuleFor(x => x.ShortName).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Properties)
            .Must(x => x.Any()).WithMessage("At least one property is required")
            .ForEach(x => x.ChildRules(prop =>
            {
                prop.RuleFor(x => x.Key).NotEmpty();
                prop.RuleFor(x => x.Value).NotEmpty();
            }));
        RuleFor(x => x.Price).GreaterThan(10);
    }
}

public record PostProductResponse(int Id);


[JsonSerializable(typeof(GetProductResponse))]
[JsonSerializable(typeof(PostProductRequest))]
[JsonSerializable(typeof(PostProductResponse))]
public partial class PublicEndpointsJsonSerializerContext : JsonSerializerContext;
