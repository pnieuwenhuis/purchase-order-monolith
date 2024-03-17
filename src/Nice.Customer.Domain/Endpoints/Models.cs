using FluentValidation;
using Nice.Core.Validation;

namespace Nice.Customer.Domain.Endpoints;

public record GetCustomerResponse(int Id, string Name, string Address, string ZipCode, string City, string Country);

[ValidateRequest(typeof(PostCustomerRequestValidator))]
public record PostCustomerRequest(int? Id, string Name, string Address, string ZipCode, string City, string Country);

public class PostCustomerRequestValidator : AbstractValidator<PostCustomerRequest>
{
    public PostCustomerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
        RuleFor(x => x.ZipCode).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();
    }
}

public record PostCustomerResponse(int Id);

[JsonSerializable(typeof(GetCustomerResponse))]
[JsonSerializable(typeof(PostCustomerRequest))]
[JsonSerializable(typeof(PostCustomerResponse))]
public partial class PublicEndpointsJsonSerializerContext : JsonSerializerContext;
