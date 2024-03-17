using Nice.Core.Web;
using Nice.Customer.Domain.Services;

namespace Nice.Customer.Domain.Endpoints;

[RegisterSingleton<IPublicEndpoints>(Duplicate = DuplicateStrategy.Append)]
public class PublicEndpoints(ICustomerService customerService) : IPublicEndpoints
{
    public string RoutePrefix => "/customers";

    public void RegisterEndpoints(RouteGroupBuilder builder)
    {
        builder.MapGet("{id}", GetCustomerById)
            .WithOpenApi()
            .WithDescription("Get a customer by identifier and returns the full details of the customer")
            .WithDisplayName("Get customer by identifier")
            .WithTags("Customers");

        builder.MapPost(string.Empty, InsertCustomerAsync)
            .WithOpenApi()
            .WithDescription("Inserts a new customer and return the generated identifier")
            .WithDisplayName("Insert customer")
            .WithTags("Customers");

        builder.MapDelete("{id}", DeleteCustomerAsync)
            .WithOpenApi()
            .WithDescription("Deletes a customer by identifier if it exists")
            .WithDisplayName("Delete customer")
            .WithTags("Customers");
    }

    public async Task<IResult> GetCustomerById(int id) =>
        await customerService.GetCustomerAsync(id) switch
        {
            CustomerService.CustomerFound(var customer) => Results.Ok(SuccessDetails.From(customer.ToResponse())),
            CustomerService.CustomerNotFound => Results.NotFound(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IResult> InsertCustomerAsync(PostCustomerRequest customer) =>
        await customerService.InsertCustomerAsync(customer.ToCustomerModel()) switch
        {
            CustomerService.CustomerInserted i => Results.Ok(SuccessDetails.From(i.ToResponse())),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IResult> DeleteCustomerAsync(int id) =>
        await customerService.DeleteCustomerAsync(id) switch
        {
            CustomerService.CustomerDeleted => Results.Accepted(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };
}
