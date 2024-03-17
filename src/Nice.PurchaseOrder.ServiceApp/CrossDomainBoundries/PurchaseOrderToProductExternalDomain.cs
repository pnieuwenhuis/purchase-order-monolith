using Nice.Product.Domain.Services;
using External = Nice.PurchaseOrder.Domain.External;

namespace Nice.PurchaseOrder.ServiceApp.CrossDomainBoundries
{
    [RegisterSingleton<External.IProductExternalDomain>]
    public class PurchaseOrderToProductExternalDomain(IProductService productService) : External.IProductExternalDomain
    {
        public async Task<External.IGetProductsByIdResponse> GetProductsByIdAsync(int[] ids)
        {
            var response = await productService.GetProductsAsync(ids);
            return response switch
            {
                ProductService.ProductsFound(var products) => new External.ProductsFound(products.Select(
                    p => new External.Product(p.Id!.Value, p.Description, p.Price))),
                ProductService.OperationFailure => new External.ProductOperationFailure(),
                _ => throw new InvalidOperationException($"Unexpected response type: {response.GetType().Name}")
            };
        }
    }
}
