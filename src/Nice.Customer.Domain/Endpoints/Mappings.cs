using Nice.Customer.Domain.Services;
using Riok.Mapperly.Abstractions;

namespace Nice.Customer.Domain.Endpoints;

[Mapper]
public static partial class EndpointMappers
{
    public static partial CustomerModel ToCustomerModel(this PostCustomerRequest customer);
    public static partial GetCustomerResponse ToResponse(this CustomerModel customer);
    public static partial PostCustomerResponse ToResponse(this CustomerService.CustomerInserted inserted);
}
