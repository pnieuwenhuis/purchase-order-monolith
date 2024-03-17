using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Nice.Core.Web;
using Nice.PurchaseOrder.Domain.Endpoints;
using PurchaseOrderServices = Nice.PurchaseOrder.Domain.Services;
using Xunit;

namespace Nice.PurchaseOrder.Domain.Tests.Endpoints;

public class PublicEndpointsTests
{
    [Fact]
    public async Task GetPurchaseOrderById_Should_Return_200OK_When_Found()
    {
        var purchaseOrderServiceMock = new Mock<PurchaseOrderServices.IPurchaseOrderService>();
        var subject = new PublicEndpoints(purchaseOrderServiceMock.Object);

        var customerId = 1;
        purchaseOrderServiceMock.Setup(x => x.GetPurchaseOrderAsync(customerId))
            .ReturnsAsync(new PurchaseOrderServices.PurchaseOrderService.PurchaseOrderFound(TestData.PurchaseOrderFound));

        var result = await subject.GetPurchaseOrderById(customerId);

        result.Should()
            .BeOfType<Ok<SuccessDetails>>().Which.Value.Should()
            .BeEquivalentTo(
                SuccessDetails.From(
                    new PurchaseOrderResponse(
                        Id: 1,
                        CustomerId: 1,
                        CustomerName: "John Doe",
                        Items: [
                            new PurchaseOrderItemFull(Id: 1, ProductId: 1, ProductName: "Product 1", Quantity: 1, Price: 100),
                            new PurchaseOrderItemFull(Id: 2, ProductId: 2, ProductName: "Product 2", Quantity: 1, Price: 200)
                        ],
                        TotalPrice: 300)));
        purchaseOrderServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetPurchaseOrderById_Should_Return_404NotFound_When_Not_Found()
    {
        var purchaseOrderServiceMock = new Mock<PurchaseOrderServices.IPurchaseOrderService>();
        var subject = new PublicEndpoints(purchaseOrderServiceMock.Object);

        var customerId = 1;
        purchaseOrderServiceMock.Setup(x => x.GetPurchaseOrderAsync(customerId))
            .ReturnsAsync(new PurchaseOrderServices.PurchaseOrderService.PurchaseOrderNotFound());

        var result = await subject.GetPurchaseOrderById(customerId);

        result.Should().BeOfType<NotFound>();
        purchaseOrderServiceMock.VerifyAll();
    }

    [Fact]
    public async Task InsertPurchaseOrderAsync_Should_Return_200OK_When_Inserted()
    {
        var purchaseOrderServiceMock = new Mock<PurchaseOrderServices.IPurchaseOrderService>();
        var subject = new PublicEndpoints(purchaseOrderServiceMock.Object);
        var input = new PurchaseOrderRequest(
            CustomerId: 1,
            Items: [
                new PurchaseOrderItemShort(Id: 1, ProductId: 1, Quantity: 1),
                new PurchaseOrderItemShort(Id: 2, ProductId: 2, Quantity: 1)
            ]
        );

        var id = 1;
        PurchaseOrderServices.NewPurchaseOrderModel newPurchaseOrder;
        purchaseOrderServiceMock.Setup(x => x.InsertPurchaseOrderAsync(It.IsAny<PurchaseOrderServices.NewPurchaseOrderModel>()))
            .Callback<PurchaseOrderServices.NewPurchaseOrderModel>(x => newPurchaseOrder = x)
            .ReturnsAsync(new PurchaseOrderServices.PurchaseOrderService.PurchaseOrderInserted(id));

        var result = await subject.InsertPurchaseOrderAsync(input);

        result.Should()
            .BeOfType<Ok<SuccessDetails>>().Which.Value.Should()
            .BeEquivalentTo(
                SuccessDetails.From(new PostPurchaseOrderResponse(id)));
        purchaseOrderServiceMock.VerifyAll();
    }

    [Fact]
    public async Task DeletePurchaseOrderAsync_Should_Return_200OK_When_Inserted()
    {
        var purchaseOrderServiceMock = new Mock<PurchaseOrderServices.IPurchaseOrderService>();
        var subject = new PublicEndpoints(purchaseOrderServiceMock.Object);

        var customerId = 1;
        purchaseOrderServiceMock.Setup(x => x.DeletePurchaseOrderAsync(customerId))
            .ReturnsAsync(new PurchaseOrderServices.PurchaseOrderService.PurchaseOrderDeleted());

        var result = await subject.DeletePurchaseOrderAsync(customerId);

        result.Should()
            .BeOfType<Accepted>();
        purchaseOrderServiceMock.VerifyAll();
    }
}
