using System.Net.Http.Json;
using System.Text;
using Xunit;
using System.Text.Json;
using Nice.Product.Domain.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nice.Postgres.Data;
using Nice.Product.Domain.Endpoints;
using System.Net;

namespace Nice.PurchaseOrder.IntegationTests;

[Collection("Integration tests")]
public class ProductTests
{
    private readonly ServiceFixure fixure;

    public ProductTests(ServiceFixure fixure)
    {
        this.fixure = fixure;
    }

    [Fact]
    public async Task Store_Product()
    {
        // Arrange
        var client = fixure.Factory.CreateClient();
        IProductDbRepository repository = fixure.Factory.Services.GetRequiredService<IProductDbRepository>();
        var body = """
        {
            "short_name": "Phone",
            "description": "A thing to call people",
            "properties": {
                "Color": "Red",
                "Size": "Small"
            },
            "price": 9999
        }
        """;

        // Act
        var response = await client.PostAsync("/public/products", new StringContent(body, Encoding.UTF8, "application/json"));
        var output = await response.Content.ReadFromJsonAsync<SuccessResponse<PostProductResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResult = await repository.GetProductAsync(output!.Data.Id);
        getResult.Should().BeOfType<DbSingleRowResult<Models.ProductDbModel>>()
            .Which.Row.Should().BeEquivalentTo(productDbModel with { Id = output.Data.Id });
    }

    [Fact]
    public async Task Get_Product()
    {
        // Arrange
        var client = fixure.Factory.CreateClient();
        IProductDbRepository repository = fixure.Factory.Services.GetRequiredService<IProductDbRepository>();
        var insertResult = await repository.InsertProductAsync(productDbModel);
        if (insertResult is not DbSingleRowResult<int>(int id))
        {
            throw new Exception("Insertion failed");
        }

        // Act
        var response = await client.GetAsync($"/public/products/{id}");
        var output = await response.Content.ReadFromJsonAsync<SuccessResponse<GetProductResponse>>(
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output!.Data.Should().BeEquivalentTo(
            new GetProductResponse(
                Id: id,
                ShortName: "Phone",
                Description: "A thing to call people",
                Properties: new Dictionary<string, string>
                {
                    { "Color", "Red" },
                    { "Size", "Small" }
                },
                Price: 9999));
    }

    [Fact]
    public async Task Delete_Product()
    {
        // Arrange
        var client = fixure.Factory.CreateClient();
        IProductDbRepository repository = fixure.Factory.Services.GetRequiredService<IProductDbRepository>();
        var insertResult = await repository.InsertProductAsync(productDbModel);
        if (insertResult is not DbSingleRowResult<int>(int id))
        {
            throw new Exception("Insertion failed");
        }

        // Act
        var response = await client.DeleteAsync($"/public/products/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var getResult = await repository.GetProductAsync(id);
        getResult.Should().BeOfType<DbEmptyResult<Models.ProductDbModel>>();
    }

    private Models.ProductDbModel productDbModel = new(
        Id: null,
        ShortName: "Phone",
        Description: "A thing to call people",
        Properties: new Dictionary<string, string>
        {
            { "Color", "Red" },
            { "Size", "Small" }
        },
        Price: 9999);
}