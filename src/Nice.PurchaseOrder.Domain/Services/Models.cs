namespace Nice.PurchaseOrder.Domain.Services;

public record NewPurchaseOrderModel(int CustomerId, IEnumerable<PurchaseOrderItemShort> Items);

public record PurchaseOrderModel(int Id, int CustomerId, string CustomerName, IEnumerable<PurchaseOrderItemFull> Items)
{
    public int TotalPrice => Items.Sum(item => item.Price * item.Quantity);
}

public record PurchaseOrderItemFull(int Id, int ProductId, string ProductName, int Quantity, int Price);

public record PurchaseOrderItemShort(int Id, int ProductId, int Quantity);
