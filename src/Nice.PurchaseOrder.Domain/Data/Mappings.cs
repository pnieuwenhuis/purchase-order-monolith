using Riok.Mapperly.Abstractions;

namespace Nice.PurchaseOrder.Domain.Data;

[Mapper]
public static partial class DataMappers
{
    public static partial Documents.PurchaseOrder ToPurchaseOrderDocument(this Models.PurchaseOrderDbModel order);
    public static Models.PurchaseOrderDbModel ToPurchaseOrderDbModel(this (int Id, Documents.PurchaseOrder Order) t) =>
        new(t.Id, t.Order.Customer.ToCustomerDetailsDbModel(), t.Order.Items.Select(x => x.ToOrderItemDbModel()));

    public static partial Models.CustomerDetails ToCustomerDetailsDbModel(this Documents.CustomerDetails customer);
    public static partial Models.PurchaseOrderItem ToOrderItemDbModel(this Documents.PurchaseOrderItem item);

}
