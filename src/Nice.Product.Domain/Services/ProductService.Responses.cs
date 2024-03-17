namespace Nice.Product.Domain.Services;

[RegisterSingleton<IProductService>]
public partial class ProductService
{
    public interface IGetProductResponse;
    public record ProductFound(ProductModel Product) : IGetProductResponse;
    public record ProductNotFound : IGetProductResponse;

    public interface IInsertProductResponse;
    public record ProductInserted(int id) : IInsertProductResponse;

    public interface IDeleteProductResponse;
    public record ProductDeleted : IDeleteProductResponse;

    public interface IGetProductsResponse;
    public record ProductsFound(IEnumerable<ProductModel> Product) : IGetProductsResponse;

    public record OperationFailure : IGetProductResponse, IGetProductsResponse, IDeleteProductResponse, IInsertProductResponse;
}
