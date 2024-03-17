namespace Nice.Customer.Domain.Services;

public interface ICustomerService
{
    Task<CustomerService.IInsertCustomerResponse> InsertCustomerAsync(CustomerModel customer);
    Task<CustomerService.IGetCustomerResponse> GetCustomerAsync(int id);
    Task<CustomerService.IDeleteCustomerResponse> DeleteCustomerAsync(int id);
}
