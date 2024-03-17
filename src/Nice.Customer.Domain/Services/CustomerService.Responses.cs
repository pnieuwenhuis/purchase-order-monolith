namespace Nice.Customer.Domain.Services;

public partial class CustomerService
{
    public interface IGetCustomerResponse;
    public record CustomerFound(CustomerModel Customer) : IGetCustomerResponse;
    public record CustomerNotFound : IGetCustomerResponse;

    public interface IInsertCustomerResponse;
    public record CustomerInserted(int id) : IInsertCustomerResponse;

    public interface IDeleteCustomerResponse;
    public record CustomerDeleted : IDeleteCustomerResponse;

    public record OperationFailure : IDeleteCustomerResponse, IInsertCustomerResponse, IGetCustomerResponse;
}
