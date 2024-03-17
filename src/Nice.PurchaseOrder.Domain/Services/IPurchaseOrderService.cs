namespace Nice.PurchaseOrder.Domain.Services;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderService.IInsertPurchaseOrderResponse> InsertPurchaseOrderAsync(NewPurchaseOrderModel purchaseOrder);
    Task<PurchaseOrderService.IGetPurchaseOrderResponse> GetPurchaseOrderAsync(int id);
    Task<PurchaseOrderService.IDeletePurchaseOrderResponse> DeletePurchaseOrderAsync(int id);
}
