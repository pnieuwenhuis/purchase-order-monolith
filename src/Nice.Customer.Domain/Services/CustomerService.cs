using Nice.Customer.Domain.Data;
using Nice.Postgres.Data;

namespace Nice.Customer.Domain.Services;

[RegisterSingleton<ICustomerService>]
public partial class CustomerService(ICustomerDbRepository customerRepository) : ICustomerService
{
    public async Task<IInsertCustomerResponse> InsertCustomerAsync(CustomerModel customer) =>
        await customerRepository.InsertCustomerAsync(customer.ToCustomerDbModel()) switch
        {
            DbSingleRowResult<int>(var id) => new CustomerInserted(id),
            DbNonSuccessResult<int> => new OperationFailure(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IGetCustomerResponse> GetCustomerAsync(int id) =>
        await customerRepository.GetCustomerAsync(id) switch
        {
            DbEmptyResult<Models.CustomerDbModel> => new CustomerNotFound(),
            DbSingleRowResult<Models.CustomerDbModel>(var data) => new CustomerFound(data.ToCustomerModel()),
            DbNonSuccessResult<Models.CustomerDbModel> => new OperationFailure(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IDeleteCustomerResponse> DeleteCustomerAsync(int id) =>
        await customerRepository.DeleteCustomerAsync(id) switch
        {
            DbSingleRowResult<int> => new CustomerDeleted(),
            DbNonSuccessResult<int> => new OperationFailure(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };
}
