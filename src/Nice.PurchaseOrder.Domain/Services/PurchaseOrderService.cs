using Nice.Core.Exceptions;
using Nice.Postgres.Data;
using Data = Nice.PurchaseOrder.Domain.Data;
using Nice.PurchaseOrder.Domain.External;

namespace Nice.PurchaseOrder.Domain.Services;

[RegisterSingleton<IPurchaseOrderService>]
public partial class PurchaseOrderService(
    Data.IPurchaseOrderDbRepository purchaseOrderRepository,
    ICustomerExternalDomain customerExternalDomain,
    IProductExternalDomain productExternalDomain) : IPurchaseOrderService
{
    public async Task<IDeletePurchaseOrderResponse> DeletePurchaseOrderAsync(int id) =>
        await purchaseOrderRepository.DeletePurchaseOrderAsync(id) switch
        {
            DbSingleRowResult<int> => new PurchaseOrderDeleted(),
            DbNonSuccessResult<int> => throw new ServiceException($"Could not delete order {id}"),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IGetPurchaseOrderResponse> GetPurchaseOrderAsync(int id) =>
        await purchaseOrderRepository.GetPurchaseOrderAsync(id) switch
        {
            DbEmptyResult<Data.Models.PurchaseOrderDbModel> => new PurchaseOrderNotFound(),
            DbSingleRowResult<Data.Models.PurchaseOrderDbModel>(var data) => new PurchaseOrderFound(data.ToFoundPurchaseOrder()),
            DbNonSuccessResult<Data.Models.PurchaseOrderDbModel> => throw new ServiceException($"Could not retrieve order {id}"),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IInsertPurchaseOrderResponse> InsertPurchaseOrderAsync(NewPurchaseOrderModel purchaseOrder)
    {
        var customer = await customerExternalDomain.GetCustomerByIdAsync(purchaseOrder.CustomerId);
        if (customer is not CustomerFound customerFound)
        {
            return new NotInsertedCustomerNotFound();
        }

        var products = await productExternalDomain.GetProductsByIdAsync(purchaseOrder.Items.Select(item => item.ProductId).ToArray());
        if (products is ProductsFound productsFound)
        {
            if (productsFound.Products.Count() != purchaseOrder.Items.Count())
            {
                return new NotInsertedProductsNotFound(purchaseOrder.Items
                    .Select(item => item.ProductId)
                    .Where(id => !productsFound.Products.Any(product => product.Id == id)));
            }
        }
        else
        {
            return new NotInsertedProductsNotFound(purchaseOrder.Items.Select(item => item.ProductId));
        }

        return await purchaseOrderRepository.InsertPurchaseOrderAsync((purchaseOrder, customerFound, productsFound.Products).ToPurchaseOrderDbModel()) switch
        {
            DbSingleRowResult<int>(var id) => new PurchaseOrderInserted(id),
            DbNonSuccessResult<int> => throw new ServiceException($"Could not insert order"),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };
    }
}
