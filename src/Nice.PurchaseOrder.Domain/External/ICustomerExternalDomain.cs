namespace Nice.PurchaseOrder.Domain.External;

public interface IGetCustomerByIdResponse;
public record CustomerFound(int Id, string Name, string Address, string ZipCode, string City, string Country) : IGetCustomerByIdResponse;
public record CustomerNotFound : IGetCustomerByIdResponse;
public record CustomerOperationFailure : IGetCustomerByIdResponse;

public interface ICustomerExternalDomain
{
    Task<IGetCustomerByIdResponse> GetCustomerByIdAsync(int id);
}
