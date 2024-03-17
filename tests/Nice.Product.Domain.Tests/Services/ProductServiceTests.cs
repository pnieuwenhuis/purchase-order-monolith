using FluentAssertions;
using Moq;
using Nice.Core.Exceptions;
using Nice.Product.Domain.Data;
using Nice.Product.Domain.Services;
using Nice.Postgres.Data;
using Xunit;

namespace Nice.Product.Domain.Tests.Services;

public class ProductServiceTests
{
    [Fact]
    public async Task InsertProductAsync_Should_Return_ProductInserted_When_Success()
    {
        var productDbRepositoryMock = new Mock<IProductDbRepository>();
        var subject = new ProductService(productDbRepositoryMock.Object);

        Models.ProductDbModel? productDbModel = null;
        productDbRepositoryMock.Setup(x => x.InsertProductAsync(It.IsAny<Models.ProductDbModel>()))
            .Callback<Models.ProductDbModel>(c => productDbModel = c)
            .ReturnsAsync(DbResult.SingleRow(1));

        var result = await subject.InsertProductAsync(TestData.Product);

        result.Should()
            .BeOfType<ProductService.ProductInserted>().And
            .BeEquivalentTo(new ProductService.ProductInserted(1));
        productDbRepositoryMock.VerifyAll();
        productDbModel.Should().BeEquivalentTo(TestData.ProductDbModel);
    }

    [Fact]
    public async Task InsertProductAsync_Should_Return_DbNonSuccessResult_When_Failed()
    {
        var productDbRepositoryMock = new Mock<IProductDbRepository>();
        var subject = new ProductService(productDbRepositoryMock.Object);

        productDbRepositoryMock.Setup(x => x.InsertProductAsync(It.IsAny<Models.ProductDbModel>()))
            .ReturnsAsync(DbResult.NonSuccess<int>());

        var result = await subject.InsertProductAsync(TestData.Product);

        result.Should().BeOfType<ProductService.OperationFailure>();
        productDbRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetProductAsync_Should_Return_ProductFound_When_Found()
    {
        var productDbRepositoryMock = new Mock<IProductDbRepository>();
        var subject = new ProductService(productDbRepositoryMock.Object);

        int productId = 1;
        productDbRepositoryMock.Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(DbResult.SingleRow(TestData.ProductDbModel));

        var result = await subject.GetProductAsync(productId);

        result.Should()
            .BeOfType<ProductService.ProductFound>().And
            .BeEquivalentTo(new ProductService.ProductFound(TestData.Product));
        productDbRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetProductAsync_Should_Return_ProductNotFound_When_Not_Found()
    {
        var productDbRepositoryMock = new Mock<IProductDbRepository>();
        var subject = new ProductService(productDbRepositoryMock.Object);

        int productId = 1;
        productDbRepositoryMock.Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(DbResult.Empty<Models.ProductDbModel>());

        var result = await subject.GetProductAsync(productId);

        result.Should()
            .BeOfType<ProductService.ProductNotFound>();
        productDbRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetProductAsync_Should_Throw_ServiceException_When_Non_Success()
    {
        var productDbRepositoryMock = new Mock<IProductDbRepository>();
        var subject = new ProductService(productDbRepositoryMock.Object);

        int productId = 1;
        productDbRepositoryMock.Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(DbResult.NonSuccess<Models.ProductDbModel>());

        var result = await subject.GetProductAsync(productId);

        result.Should().BeOfType<ProductService.OperationFailure>();
        productDbRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteProductAsync_Should_Return_ProductDeleted_When_Deleted()
    {
        var productDbRepositoryMock = new Mock<IProductDbRepository>();
        var subject = new ProductService(productDbRepositoryMock.Object);

        int productId = 1;
        productDbRepositoryMock.Setup(x => x.DeleteProductAsync(productId))
            .ReturnsAsync(DbResult.SingleRow(1));

        var result = await subject.DeleteProductAsync(productId);

        result.Should()
            .BeOfType<ProductService.ProductDeleted>();
        productDbRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteProductAsync_Should_Throw_ServiceException_When_Non_Success()
    {
        var productDbRepositoryMock = new Mock<IProductDbRepository>();
        var subject = new ProductService(productDbRepositoryMock.Object);

        int productId = 1;
        productDbRepositoryMock.Setup(x => x.DeleteProductAsync(productId))
            .ReturnsAsync(DbResult.NonSuccess<int>());

        var result = await subject.DeleteProductAsync(productId);

        result.Should().BeOfType<ProductService.OperationFailure>();
        productDbRepositoryMock.VerifyAll();
    }
}
