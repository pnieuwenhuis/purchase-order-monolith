namespace Nice.Product.Domain.Services;

public interface IProductService
{
    Task<ProductService.IInsertProductResponse> InsertProductAsync(ProductModel product);
    Task<ProductService.IGetProductResponse> GetProductAsync(int id);
    Task<ProductService.IGetProductsResponse> GetProductsAsync(int[] ids);
    Task<ProductService.IDeleteProductResponse> DeleteProductAsync(int id);
}
