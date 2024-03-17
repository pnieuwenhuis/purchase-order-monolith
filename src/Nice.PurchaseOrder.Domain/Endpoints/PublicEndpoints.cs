using Nice.Core.Endpoints;
using Nice.Core.Validation;
using Nice.Core.Web;
using Nice.PurchaseOrder.Domain.Services;

namespace Nice.PurchaseOrder.Domain.Endpoints;

[RegisterSingleton<IPublicEndpoints>(Duplicate = DuplicateStrategy.Append)]
public class PublicEndpoints(IPurchaseOrderService purchaseOrderService) : IPublicEndpoints
{
    public string RoutePrefix => "/purchase-orders";

    public void RegisterEndpoints(RouteGroupBuilder builder)
    {
        builder.MapGet("{id}", GetPurchaseOrderById)
            .WithOpenApi()
            .WithDescription("Get a purchase order by identifier and returns the full details of the order")
            .WithDisplayName("Get purchase order by identifier")
            .WithTags("Purchase Orders");

        builder.MapPost(string.Empty, InsertPurchaseOrderAsync)
            .WithOpenApi()
            .WithDescription("Inserts a new purchase order and return the generated identifier")
            .WithDisplayName("Insert purchase order")
            .WithTags("Purchase Orders");

        builder.MapDelete("{id}", DeletePurchaseOrderAsync)
            .WithOpenApi()
            .WithDescription("Deletes a purchase order by identifier if it exists")
            .WithDisplayName("Delete purchase order")
            .WithTags("Purchase Orders");
    }

    public async Task<IResult> GetPurchaseOrderById(int id) =>
        await purchaseOrderService.GetPurchaseOrderAsync(id) switch
        {
            PurchaseOrderService.PurchaseOrderFound(var purchaseOrder) => Results.Ok(SuccessDetails.From(purchaseOrder.ToResponse())),
            PurchaseOrderService.PurchaseOrderNotFound => Results.NotFound(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IResult> InsertPurchaseOrderAsync(PurchaseOrderRequest purchaseOrder) =>
        await purchaseOrderService.InsertPurchaseOrderAsync(purchaseOrder.ToNewPurchaseOrder()) switch
        {
            PurchaseOrderService.PurchaseOrderInserted i => Results.Ok(SuccessDetails.From(new PostPurchaseOrderResponse(i.Id))),
            PurchaseOrderService.NotInsertedCustomerNotFound => (nameof(PurchaseOrderRequest.CustomerId), "CUSTOMER_NOT_FOUND").ToValidationProblem(),
            PurchaseOrderService.NotInsertedProductsNotFound(var products) => (nameof(PurchaseOrderRequest.Items), $"PRODUCTS_NOT_FOUND ({string.Join(", ", products)})").ToValidationProblem(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IResult> DeletePurchaseOrderAsync(int id) =>
        await purchaseOrderService.DeletePurchaseOrderAsync(id) switch
        {
            PurchaseOrderService.PurchaseOrderDeleted => Results.Accepted(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };
}
