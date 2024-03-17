using FluentAssertions;
using Moq;
using Nice.Core.Exceptions;
using Nice.PurchaseOrder.Domain.Services;
using Nice.Postgres.Data;
using Xunit;
using Nice.PurchaseOrder.Domain.External;

namespace Nice.PurchaseOrder.Domain.Tests.Services;

public class PurchaseOrderServiceTests
{
    [Fact]
    public async Task InsertPurchaseOrderAsync_Should_Return_PurchaseOrderInserted_When_Success()
    {
        var purchaseOrderDbRepository = new Mock<Data.IPurchaseOrderDbRepository>();
        var customerExternalDomain = new Mock<ICustomerExternalDomain>();
        var productExternalDomain = new Mock<IProductExternalDomain>();
        var subject = new PurchaseOrderService(purchaseOrderDbRepository.Object, customerExternalDomain.Object, productExternalDomain.Object);

        Data.Models.PurchaseOrderDbModel? purchaseOrderDbModel = null;
        customerExternalDomain.Setup(x => x.GetCustomerByIdAsync(1))
            .ReturnsAsync(new CustomerFound(1, "John Doe", "123 Main St", "12345", "Anytown", "NY"));
        productExternalDomain.Setup(x => x.GetProductsByIdAsync(new[] { 1, 2 }))
            .ReturnsAsync(new ProductsFound([
                new Product(1, "Product 1", 100),
                new Product(2, "Product 2", 200)
            ]));
        purchaseOrderDbRepository.Setup(x => x.InsertPurchaseOrderAsync(It.IsAny<Data.Models.PurchaseOrderDbModel>()))
            .Callback<Data.Models.PurchaseOrderDbModel>(c => purchaseOrderDbModel = c)
            .ReturnsAsync(DbResult.SingleRow(1));

        var result = await subject.InsertPurchaseOrderAsync(TestData.NewPurchaseOrder);

        result.Should()
            .BeOfType<PurchaseOrderService.PurchaseOrderInserted>().And
            .BeEquivalentTo(new PurchaseOrderService.PurchaseOrderInserted(1));
        purchaseOrderDbRepository.VerifyAll();
        purchaseOrderDbModel.Should().BeEquivalentTo(
            new Data.Models.PurchaseOrderDbModel(
                Id: default,
                Customer: new Data.Models.CustomerDetails(1, "John Doe", "123 Main St", "Anytown", "12345", "NY"),
                Items: [
                    new Data.Models.PurchaseOrderItem(
                        Id: 1,
                        Product: new Data.Models.ProductDetails(1, "Product 1", 100),
                        Quantity: 1),
                    new Data.Models.PurchaseOrderItem(
                        Id: 2,
                        Product: new Data.Models.ProductDetails(2, "Product 2", 200),
                        Quantity: 1)
                ]));
    }

    [Fact]
    public async Task InsertPurchaseOrderAsync_Should_Return_DbNonSuccessResult_When_Failed()
    {
        var purchaseOrderDbRepository = new Mock<Data.IPurchaseOrderDbRepository>();
        var customerExternalDomain = new Mock<ICustomerExternalDomain>();
        var productExternalDomain = new Mock<IProductExternalDomain>();
        var subject = new PurchaseOrderService(purchaseOrderDbRepository.Object, customerExternalDomain.Object, productExternalDomain.Object);

        customerExternalDomain.Setup(x => x.GetCustomerByIdAsync(1))
            .ReturnsAsync(new CustomerFound(1, "John Doe", "123 Main St", "12345", "Anytown", "NY"));
        productExternalDomain.Setup(x => x.GetProductsByIdAsync(new[] { 1, 2 }))
            .ReturnsAsync(new ProductsFound([
                new Product(1, "Product 1", 100),
                new Product(2, "Product 2", 200)
            ]));
        purchaseOrderDbRepository.Setup(x => x.InsertPurchaseOrderAsync(It.IsAny<Data.Models.PurchaseOrderDbModel>()))
            .ReturnsAsync(DbResult.NonSuccess<int>());

        var result = async () => await subject.InsertPurchaseOrderAsync(TestData.NewPurchaseOrder);

        await result.Should().ThrowAsync<ServiceException>().WithMessage("Could not insert order");
        purchaseOrderDbRepository.VerifyAll();
    }

    [Fact]
    public async Task InsertPurchaseOrderAsync_Should_Return_NotInsertedProductsNotFound_When_Product_Not_Found()
    {
        var purchaseOrderDbRepository = new Mock<Data.IPurchaseOrderDbRepository>();
        var customerExternalDomain = new Mock<ICustomerExternalDomain>();
        var productExternalDomain = new Mock<IProductExternalDomain>();
        var subject = new PurchaseOrderService(purchaseOrderDbRepository.Object, customerExternalDomain.Object, productExternalDomain.Object);

        customerExternalDomain.Setup(x => x.GetCustomerByIdAsync(1))
            .ReturnsAsync(new CustomerFound(1, "John Doe", "123 Main St", "12345", "Anytown", "NY"));
        productExternalDomain.Setup(x => x.GetProductsByIdAsync(new[] { 1, 2 }))
            .ReturnsAsync(new ProductsFound(Array.Empty<Product>()));

        var result = await subject.InsertPurchaseOrderAsync(TestData.NewPurchaseOrder);

        result.Should().BeOfType<PurchaseOrderService.NotInsertedProductsNotFound>();
        purchaseOrderDbRepository.Verify(x => x.InsertPurchaseOrderAsync(It.IsAny<Data.Models.PurchaseOrderDbModel>()), Times.Never());
    }

    [Fact]
    public async Task InsertPurchaseOrderAsync_Should_Return_NotInsertedProductsNotFound_When_Products_Cannot_Be_Retrieved()
    {
        var purchaseOrderDbRepository = new Mock<Data.IPurchaseOrderDbRepository>();
        var customerExternalDomain = new Mock<ICustomerExternalDomain>();
        var productExternalDomain = new Mock<IProductExternalDomain>();
        var subject = new PurchaseOrderService(purchaseOrderDbRepository.Object, customerExternalDomain.Object, productExternalDomain.Object);

        customerExternalDomain.Setup(x => x.GetCustomerByIdAsync(1))
            .ReturnsAsync(new CustomerFound(1, "John Doe", "123 Main St", "12345", "Anytown", "NY"));
        productExternalDomain.Setup(x => x.GetProductsByIdAsync(new[] { 1, 2 }))
            .ReturnsAsync(new ProductOperationFailure());

        var result = await subject.InsertPurchaseOrderAsync(TestData.NewPurchaseOrder);

        result.Should().BeOfType<PurchaseOrderService.NotInsertedProductsNotFound>();
        purchaseOrderDbRepository.Verify(x => x.InsertPurchaseOrderAsync(It.IsAny<Data.Models.PurchaseOrderDbModel>()), Times.Never());
    }

    [Fact]
    public async Task InsertPurchaseOrderAsync_Should_Return_NotInsertedCustomerNotFound_When_Customer_Not_Found()
    {
        var purchaseOrderDbRepository = new Mock<Data.IPurchaseOrderDbRepository>();
        var customerExternalDomain = new Mock<ICustomerExternalDomain>();
        var productExternalDomain = new Mock<IProductExternalDomain>();
        var subject = new PurchaseOrderService(purchaseOrderDbRepository.Object, customerExternalDomain.Object, productExternalDomain.Object);

        customerExternalDomain.Setup(x => x.GetCustomerByIdAsync(1))
            .ReturnsAsync(new CustomerNotFound());
        productExternalDomain.Setup(x => x.GetProductsByIdAsync(new[] { 1, 2 }))
            .ReturnsAsync(new ProductsFound([
                new Product(1, "Product 1", 100),
                new Product(2, "Product 2", 200)
            ]));

        var result = await subject.InsertPurchaseOrderAsync(TestData.NewPurchaseOrder);

        result.Should().BeOfType<PurchaseOrderService.NotInsertedCustomerNotFound>();
        purchaseOrderDbRepository.Verify(x => x.InsertPurchaseOrderAsync(It.IsAny<Data.Models.PurchaseOrderDbModel>()), Times.Never());
    }

    [Fact]
    public async Task InsertPurchaseOrderAsync_Should_Return_NotInsertedCustomerNotFound_When_Fails_To_Retrieve_Customer()
    {
        var purchaseOrderDbRepository = new Mock<Data.IPurchaseOrderDbRepository>();
        var customerExternalDomain = new Mock<ICustomerExternalDomain>();
        var productExternalDomain = new Mock<IProductExternalDomain>();
        var subject = new PurchaseOrderService(purchaseOrderDbRepository.Object, customerExternalDomain.Object, productExternalDomain.Object);

        customerExternalDomain.Setup(x => x.GetCustomerByIdAsync(1))
            .ReturnsAsync(new CustomerOperationFailure());
        productExternalDomain.Setup(x => x.GetProductsByIdAsync(new[] { 1, 2 }))
            .ReturnsAsync(new ProductsFound([
                new Product(1, "Product 1", 100),
                new Product(2, "Product 2", 200)
            ]));

        var result = await subject.InsertPurchaseOrderAsync(TestData.NewPurchaseOrder);

        result.Should().BeOfType<PurchaseOrderService.NotInsertedCustomerNotFound>();
        purchaseOrderDbRepository.Verify(x => x.InsertPurchaseOrderAsync(It.IsAny<Data.Models.PurchaseOrderDbModel>()), Times.Never());
    }

    [Fact]
    public async Task GetPurchaseOrderAsync_Should_Return_PurchaseOrderFound_When_Found()
    {
        var purchaseOrderDbRepository = new Mock<Data.IPurchaseOrderDbRepository>();
        var customerExternalDomain = new Mock<ICustomerExternalDomain>();
        var productExternalDomain = new Mock<IProductExternalDomain>();
        var subject = new PurchaseOrderService(purchaseOrderDbRepository.Object, customerExternalDomain.Object, productExternalDomain.Object);

        int customerId = 1;
        purchaseOrderDbRepository.Setup(x => x.GetPurchaseOrderAsync(customerId))
            .ReturnsAsync(DbResult.SingleRow(TestData.PurchaseOrderDbModel));

        var result = await subject.GetPurchaseOrderAsync(customerId);

        result.Should()
            .BeOfType<PurchaseOrderService.PurchaseOrderFound>().And
            .BeEquivalentTo(new PurchaseOrderService.PurchaseOrderFound(TestData.PurchaseOrderFound));
        purchaseOrderDbRepository.VerifyAll();
    }

    [Fact]
    public async Task GetPurchaseOrderAsync_Should_Return_PurchaseOrderNotFound_When_Not_Found()
    {
        var purchaseOrderDbRepository = new Mock<Data.IPurchaseOrderDbRepository>();
        var customerExternalDomain = new Mock<ICustomerExternalDomain>();
        var productExternalDomain = new Mock<IProductExternalDomain>();
        var subject = new PurchaseOrderService(purchaseOrderDbRepository.Object, customerExternalDomain.Object, productExternalDomain.Object);

        int orderId = 1;
        purchaseOrderDbRepository.Setup(x => x.GetPurchaseOrderAsync(orderId))
            .ReturnsAsync(DbResult.Empty<Data.Models.PurchaseOrderDbModel>());

        var result = await subject.GetPurchaseOrderAsync(orderId);

        result.Should()
            .BeOfType<PurchaseOrderService.PurchaseOrderNotFound>();
        purchaseOrderDbRepository.VerifyAll();
    }

    [Fact]
    public async Task GetPurchaseOrderAsync_Should_Throw_ServiceException_When_Non_Success()
    {
        var purchaseOrderDbRepository = new Mock<Data.IPurchaseOrderDbRepository>();
        var customerExternalDomain = new Mock<ICustomerExternalDomain>();
        var productExternalDomain = new Mock<IProductExternalDomain>();
        var subject = new PurchaseOrderService(purchaseOrderDbRepository.Object, customerExternalDomain.Object, productExternalDomain.Object);

        int orderId = 1;
        purchaseOrderDbRepository.Setup(x => x.GetPurchaseOrderAsync(orderId))
            .ReturnsAsync(DbResult.NonSuccess<Data.Models.PurchaseOrderDbModel>());

        var result = async () => await subject.GetPurchaseOrderAsync(orderId);

        await result.Should().ThrowAsync<ServiceException>().WithMessage($"Could not retrieve order {orderId}");
        purchaseOrderDbRepository.VerifyAll();
    }

    [Fact]
    public async Task DeletePurchaseOrderAsync_Should_Return_PurchaseOrderDeleted_When_Deleted()
    {
        var purchaseOrderDbRepository = new Mock<Data.IPurchaseOrderDbRepository>();
        var customerExternalDomain = new Mock<ICustomerExternalDomain>();
        var productExternalDomain = new Mock<IProductExternalDomain>();
        var subject = new PurchaseOrderService(purchaseOrderDbRepository.Object, customerExternalDomain.Object, productExternalDomain.Object);

        int customerId = 1;
        purchaseOrderDbRepository.Setup(x => x.DeletePurchaseOrderAsync(customerId))
            .ReturnsAsync(DbResult.SingleRow(1));

        var result = await subject.DeletePurchaseOrderAsync(customerId);

        result.Should()
            .BeOfType<PurchaseOrderService.PurchaseOrderDeleted>();
        purchaseOrderDbRepository.VerifyAll();
    }

    [Fact]
    public async Task DeleteCustomerAsync_Should_Throw_ServiceException_When_Non_Success()
    {
        var purchaseOrderDbRepository = new Mock<Data.IPurchaseOrderDbRepository>();
        var customerExternalDomain = new Mock<ICustomerExternalDomain>();
        var productExternalDomain = new Mock<IProductExternalDomain>();
        var subject = new PurchaseOrderService(purchaseOrderDbRepository.Object, customerExternalDomain.Object, productExternalDomain.Object);

        int customerId = 1;
        purchaseOrderDbRepository.Setup(x => x.DeletePurchaseOrderAsync(customerId))
            .ReturnsAsync(DbResult.NonSuccess<int>());

        var result = async () => await subject.DeletePurchaseOrderAsync(customerId);

        await result.Should().ThrowAsync<ServiceException>().WithMessage($"Could not delete order {customerId}");
        purchaseOrderDbRepository.VerifyAll();
    }
}
