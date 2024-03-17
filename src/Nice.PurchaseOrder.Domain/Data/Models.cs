namespace Nice.PurchaseOrder.Domain.Data;

public static class Models
{
    public record PurchaseOrderDbModel(int? Id, CustomerDetails Customer, IEnumerable<PurchaseOrderItem> Items);

    public record PurchaseOrderItem(int Id, ProductDetails Product, int Quantity);

    public record ProductDetails(int Id, string Name, int Price);

    public record CustomerDetails(int Id, string Name, string Address, string City, string ZipCode, string Country);

    public record GetPurchaseOrderQueryResult(int Id, string PurchaseOrderJson);
}

public static class Documents
{
    public record PurchaseOrder(CustomerDetails Customer, IEnumerable<PurchaseOrderItem> Items);

    public record PurchaseOrderItem(int Id, ProductDetails Product, int Quantity);

    public record ProductDetails(int Id, string Name, int Price);

    public record CustomerDetails(int Id, string Name, string Address, string City, string ZipCode, string Country);
}

[JsonSerializable(typeof(Documents.PurchaseOrder))]
public partial class DataJsonSerializerContext : JsonSerializerContext;