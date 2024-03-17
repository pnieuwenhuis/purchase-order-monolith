using Nice.Customer.Domain.Services;
using External = Nice.PurchaseOrder.Domain.External;

namespace Nice.PurchaseOrder.ServiceApp.CrossDomainBoundries
{
    [RegisterSingleton<External.ICustomerExternalDomain>]
    public class PurchaseOrderToCustomerExternalDomain(ICustomerService customerService) : External.ICustomerExternalDomain
    {
        public async Task<External.IGetCustomerByIdResponse> GetCustomerByIdAsync(int id)
        {
            var response = await customerService.GetCustomerAsync(id);
            return response switch
            {
                CustomerService.CustomerFound(var c) => new External.CustomerFound(c.Id!.Value, c.Name, c.Address, c.ZipCode, c.City, c.Country),
                CustomerService.CustomerNotFound => new External.CustomerNotFound(),
                CustomerService.OperationFailure => new External.CustomerOperationFailure(),
                _ => throw new InvalidOperationException($"Unexpected response type: {response.GetType().Name}")
            };
        }
    }
}
