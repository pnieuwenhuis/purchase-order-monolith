using FluentValidation;
using Nice.Core.Validation;

namespace Nice.PurchaseOrder.Domain.Endpoints;

public record PurchaseOrderResponse(
    int Id,
    int CustomerId,
    string CustomerName,
    IEnumerable<PurchaseOrderItemFull> Items,
    int TotalPrice);

[ValidateRequest(typeof(PurchaseOrderRequestValidator))]
public record PurchaseOrderRequest(int CustomerId, IEnumerable<PurchaseOrderItemShort> Items);
public record PurchaseOrderItemShort(int Id, int ProductId, int Quantity);
public record PurchaseOrderItemFull(int Id, int ProductId, string ProductName, int Quantity, int Price);

public class PurchaseOrderRequestValidator : AbstractValidator<PurchaseOrderRequest>
{
    public PurchaseOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items)
            .Must(x => x.Any()).WithMessage("At least one item is required")
            .ForEach(x => x.ChildRules(item =>
            {
                item.RuleFor(x => x.Id).GreaterThan(0);
                item.RuleFor(x => x.ProductId).GreaterThan(0);
                item.RuleFor(x => x.Quantity).InclusiveBetween(1, 20);
            }));
    }
}

public record PostPurchaseOrderResponse(int Id);

[JsonSerializable(typeof(PostPurchaseOrderResponse))]
[JsonSerializable(typeof(PurchaseOrderResponse))]
[JsonSerializable(typeof(PurchaseOrderItemShort))]
[JsonSerializable(typeof(PurchaseOrderRequest))]
[JsonSerializable(typeof(PurchaseOrderItemFull))]
public partial class PublicEndpointsJsonSerializerContext : JsonSerializerContext;
