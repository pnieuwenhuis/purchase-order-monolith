using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Nice.Core.Web;
using Nice.Product.Domain.Endpoints;
using Nice.Product.Domain.Services;
using Xunit;

namespace Nice.Product.Domain.Tests.Endpoints;

public class PublicEndpointsTests
{
    [Fact]
    public async Task GetProductById_Should_Return_200OK_When_Found()
    {
        var productServiceMock = new Mock<IProductService>();
        var subject = new PublicEndpoints(productServiceMock.Object);

        var productId = 1;
        productServiceMock.Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(new ProductService.ProductFound(TestData.Product));

        var result = await subject.GetProductById(productId);

        result.Should()
            .BeOfType<Ok<SuccessDetails>>().Which.Value.Should()
            .BeEquivalentTo(
                SuccessDetails.From(new GetProductResponse(
                    Id: 1,
                    ShortName: "Smartwatch",
                    Description: "Biggest smartwatch Pro",
                    Properties: new Dictionary<string, string>
                    {
                        { "Waterproof", "Yes" },
                        { "Battery", "24h" },
                        { "Size", "44mm" }
                    },
                    Price: 19900)));
        productServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetProductById_Should_Return_404NotFound_When_Not_Found()
    {
        var productServiceMock = new Mock<IProductService>();
        var subject = new PublicEndpoints(productServiceMock.Object);

        var productId = 1;
        productServiceMock.Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(new ProductService.ProductNotFound());

        var result = await subject.GetProductById(productId);

        result.Should().BeOfType<NotFound>();
        productServiceMock.VerifyAll();
    }

    [Fact]
    public async Task InsertProductAsync_Should_Return_200OK_When_Inserted()
    {
        var productServiceMock = new Mock<IProductService>();
        var subject = new PublicEndpoints(productServiceMock.Object);
        var input = new PostProductRequest(
            Id: null,
            ShortName: "Smartwatch",
            Description: "Biggest smartwatch Pro",
            Properties: new Dictionary<string, string>
            {
                { "Waterproof", "Yes" },
                { "Battery", "24h" },
                { "Size", "44mm" }
            },
            Price: 19900);

        var productId = 1;
        ProductModel productModel;
        productServiceMock.Setup(x => x.InsertProductAsync(It.IsAny<ProductModel>()))
            .Callback<ProductModel>(x => productModel = x)
            .ReturnsAsync(new ProductService.ProductInserted(productId));

        var result = await subject.PostProductAsync(input);

        result.Should()
            .BeOfType<Ok<SuccessDetails>>().Which.Value.Should()
            .BeEquivalentTo(
                SuccessDetails.From(new PostProductResponse(productId)));
        productServiceMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteProductAsync_Should_Return_200OK_When_Inserted()
    {
        var productServiceMock = new Mock<IProductService>();
        var subject = new PublicEndpoints(productServiceMock.Object);

        var productId = 1;
        productServiceMock.Setup(x => x.DeleteProductAsync(productId))
            .ReturnsAsync(new ProductService.ProductDeleted());

        var result = await subject.DeleteProductAsync(productId);

        result.Should()
            .BeOfType<Accepted>();
        productServiceMock.VerifyAll();
    }
}
