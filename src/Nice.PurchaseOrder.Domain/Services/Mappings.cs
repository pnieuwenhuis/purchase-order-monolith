using Data = Nice.PurchaseOrder.Domain.Data;
using Nice.PurchaseOrder.Domain.External;
using Riok.Mapperly.Abstractions;

namespace Nice.PurchaseOrder.Domain.Services;

[Mapper]
public static partial class Mappings
{
    public static Data.Models.PurchaseOrderDbModel ToPurchaseOrderDbModel(this (NewPurchaseOrderModel Order, CustomerFound Customer, IEnumerable<Product> Products) t) =>
        new(default, t.Customer.ToCustomerDbModel(), t.Order.Items.Select(i => (i.Id, t.Products.Single(p => p.Id == i.ProductId), i.Quantity).ToPurchaseOrderItemDbModel()));

    public static Data.Models.PurchaseOrderItem ToPurchaseOrderItemDbModel(this (int Id, Product Product, int Quantity) t) =>
        new(t.Id, new(t.Product.Id, t.Product.Description, t.Product.Price), t.Quantity);

    public static partial Data.Models.CustomerDetails ToCustomerDbModel(this CustomerFound customer);

    public static partial PurchaseOrderModel ToFoundPurchaseOrder(this Data.Models.PurchaseOrderDbModel order);

    public static PurchaseOrderItemFull ToFoundPurchaseOrderItem(this Data.Models.PurchaseOrderItem order) =>
        new(order.Id, order.Product.Id, order.Product.Name, order.Quantity, order.Product.Price);
}
