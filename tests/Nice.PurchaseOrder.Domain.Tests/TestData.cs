using Nice.PurchaseOrder.Domain.Services;

namespace Nice.PurchaseOrder.Domain.Tests;

public static class TestData
{
    public static PurchaseOrderModel PurchaseOrderFound =>
        new(
            Id: 1,
            CustomerId: 1,
            CustomerName: "John Doe",
            Items: [
                new PurchaseOrderItemFull(Id: 1, ProductId: 1, ProductName: "Product 1", Quantity: 1, Price: 100),
                new PurchaseOrderItemFull(Id: 2, ProductId: 2, ProductName: "Product 2", Quantity: 1, Price: 200)
            ]);

    public static Data.Models.PurchaseOrderDbModel PurchaseOrderDbModel =>
        new(
            Id: 1,
            Customer: new Data.Models.CustomerDetails(1, "John Doe", "123 Main St", "12345", "Anytown", "NY"),
            Items: [
                new Data.Models.PurchaseOrderItem(
                    Id: 1,
                    Product: new Data.Models.ProductDetails(1, "Product 1", 100),
                    Quantity: 1),
                new Data.Models.PurchaseOrderItem(
                    Id: 2,
                    Product: new Data.Models.ProductDetails(2, "Product 2", 200),
                    Quantity: 1)
            ]);

    public static NewPurchaseOrderModel NewPurchaseOrder =>
        new(
            CustomerId: 1,
            Items: [
                new PurchaseOrderItemShort(Id: 1, ProductId: 1, Quantity: 1),
                new PurchaseOrderItemShort(Id: 2, ProductId: 2, Quantity: 1)
            ]);
}