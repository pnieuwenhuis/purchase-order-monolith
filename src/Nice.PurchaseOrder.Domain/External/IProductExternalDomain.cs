namespace Nice.PurchaseOrder.Domain.External;

public interface IGetProductsByIdResponse;
public record ProductsFound(IEnumerable<Product> Products) : IGetProductsByIdResponse;
public record Product(int Id, string Description, int Price);
public record ProductOperationFailure : IGetProductsByIdResponse;

public interface IProductExternalDomain
{
    Task<IGetProductsByIdResponse> GetProductsByIdAsync(int[] ids);
}
