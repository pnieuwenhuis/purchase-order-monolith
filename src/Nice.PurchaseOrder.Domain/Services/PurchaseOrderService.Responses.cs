namespace Nice.PurchaseOrder.Domain.Services;


public partial class PurchaseOrderService
{
    public interface IGetPurchaseOrderResponse;
    public record PurchaseOrderFound(PurchaseOrderModel order) : IGetPurchaseOrderResponse;
    public record PurchaseOrderNotFound : IGetPurchaseOrderResponse;

    public interface IInsertPurchaseOrderResponse;
    public record PurchaseOrderInserted(int Id) : IInsertPurchaseOrderResponse;
    public record NotInsertedCustomerNotFound : IInsertPurchaseOrderResponse;
    public record NotInsertedProductsNotFound(IEnumerable<int> Products) : IInsertPurchaseOrderResponse;

    public interface IDeletePurchaseOrderResponse;
    public record PurchaseOrderDeleted : IDeletePurchaseOrderResponse;
}
