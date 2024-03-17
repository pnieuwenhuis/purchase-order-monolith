using Services = Nice.PurchaseOrder.Domain.Services;
using Riok.Mapperly.Abstractions;

namespace Nice.PurchaseOrder.Domain.Endpoints;

[Mapper]
public static partial class EndpointMappers
{
    public static partial Services.NewPurchaseOrderModel ToNewPurchaseOrder(this PurchaseOrderRequest purchaseOrder);
    public static partial PurchaseOrderItemShort ToShortItem(this Services.PurchaseOrderItemShort purchaseOrderItem);
    public static partial PurchaseOrderResponse ToResponse(this Services.PurchaseOrderModel purchaseOrder);
    public static partial PurchaseOrderItemFull ToFullItem(this Services.PurchaseOrderItemFull purchaseOrderItem);
}
