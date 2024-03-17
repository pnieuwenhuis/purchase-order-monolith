using Nice.Product.Domain.Data;
using Nice.Postgres.Data;

namespace Nice.Product.Domain.Services;

[RegisterSingleton<IProductService>]
public partial class ProductService(IProductDbRepository productRepository) : IProductService
{
    public async Task<IInsertProductResponse> InsertProductAsync(ProductModel product) =>
        await productRepository.InsertProductAsync(product.ToProductDbModel()) switch
        {
            DbSingleRowResult<int>(var id) => new ProductInserted(id),
            DbNonSuccessResult<int> => new OperationFailure(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IDeleteProductResponse> DeleteProductAsync(int id) =>
        await productRepository.DeleteProductAsync(id) switch
        {
            DbSingleRowResult<int> => new ProductDeleted(),
            DbNonSuccessResult<int> => new OperationFailure(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IGetProductResponse> GetProductAsync(int id) =>
        await productRepository.GetProductAsync(id) switch
        {
            DbEmptyResult<Models.ProductDbModel> => new ProductNotFound(),
            DbSingleRowResult<Models.ProductDbModel>(var data) => new ProductFound(data.ToProductModel()),
            DbNonSuccessResult<Models.ProductDbModel> => new OperationFailure(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IGetProductsResponse> GetProductsAsync(int[] ids) =>
        await productRepository.GetProductsAsync(ids) switch
        {
            DbManyRowsResult<Models.ProductDbModel>(var data) => new ProductsFound(data.Select(p => p.ToProductModel())),
            DbNonSuccessResult<Models.ProductDbModel> => new OperationFailure(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };
}
