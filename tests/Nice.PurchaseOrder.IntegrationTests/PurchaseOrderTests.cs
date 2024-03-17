using System.Net.Http.Json;
using System.Text;
using Xunit;
using System.Text.Json;
using Nice.PurchaseOrder.Domain.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nice.Postgres.Data;
using Nice.PurchaseOrder.Domain.Endpoints;
using System.Net;
using PurchaseOrderModels = Nice.PurchaseOrder.Domain.Data.Models;
using ProductModels = Nice.Product.Domain.Data.Models;
using CustomerModels = Nice.Customer.Domain.Data.Models;
using Nice.Customer.Domain.Data;
using Nice.Product.Domain.Data;

namespace Nice.PurchaseOrder.IntegationTests;

[Collection("Integration tests")]
public class PurchaseOrderTests
{
    private readonly ServiceFixure fixure;

    public PurchaseOrderTests(ServiceFixure fixure)
    {
        this.fixure = fixure;
    }

    [Fact]
    public async Task Store_PurchaseOrder()
    {
        // Arrange
        var client = fixure.Factory.CreateClient();
        IPurchaseOrderDbRepository repository = fixure.Factory.Services.GetRequiredService<IPurchaseOrderDbRepository>();
        ICustomerDbRepository customerRepository = fixure.Factory.Services.GetRequiredService<ICustomerDbRepository>();
        IProductDbRepository productDbRepository = fixure.Factory.Services.GetRequiredService<IProductDbRepository>();
        if (await productDbRepository.InsertProductAsync(productDbModel) is not DbSingleRowResult<int>(int productId))
        {
            throw new Exception("Product insertion failed");
        }

        if (await customerRepository.InsertCustomerAsync(customerDbModel) is not DbSingleRowResult<int>(int customerId))
        {
            throw new Exception("Customer insertion failed");
        }

        var body = $$"""
        {
            "customer_id": {{customerId}},
            "items": [
                {
                    "id": 1,
                    "product_id": {{productId}},
                    "quantity": 1
                }
            ]
        }
        """;

        // Act
        var response = await client.PostAsync("/public/purchase-orders", new StringContent(body, Encoding.UTF8, "application/json"));
        var output = await response.Content.ReadFromJsonAsync<SuccessResponse<PostPurchaseOrderResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResult = await repository.GetPurchaseOrderAsync(output!.Data.Id);
        getResult.Should().BeOfType<DbSingleRowResult<PurchaseOrderModels.PurchaseOrderDbModel>>()
            .Which.Row.Should().BeEquivalentTo(
                purchaseOrderDbModel with
                {
                    Id = output.Data.Id,
                    Customer = purchaseOrderDbModel.Customer with { Id = customerId },
                    Items = [
                        purchaseOrderDbModel.Items.First() with { Product = purchaseOrderDbModel.Items.First().Product with { Id = productId } }
                    ]
                });
    }

    [Fact]
    public async Task Get_PurchaseOrder()
    {
        // Arrange
        var client = fixure.Factory.CreateClient();
        IPurchaseOrderDbRepository repository = fixure.Factory.Services.GetRequiredService<IPurchaseOrderDbRepository>();
        ICustomerDbRepository customerRepository = fixure.Factory.Services.GetRequiredService<ICustomerDbRepository>();
        IProductDbRepository productDbRepository = fixure.Factory.Services.GetRequiredService<IProductDbRepository>();
        if (await productDbRepository.InsertProductAsync(productDbModel) is not DbSingleRowResult<int>(int productId))
        {
            throw new Exception("Product insertion failed");
        }

        if (await customerRepository.InsertCustomerAsync(customerDbModel) is not DbSingleRowResult<int>(int customerId))
        {
            throw new Exception("Customer insertion failed");
        }
        var toBeInserted = purchaseOrderDbModel with
        {
            Customer = purchaseOrderDbModel.Customer with { Id = customerId },
            Items = [
                purchaseOrderDbModel.Items.First() with { Product = purchaseOrderDbModel.Items.First().Product with { Id = productId } }
            ]
        };
        if (await repository.InsertPurchaseOrderAsync(toBeInserted) is not DbSingleRowResult<int>(int id))
        {
            throw new Exception("Purchase Order insertion failed");
        }

        // Act
        var response = await client.GetAsync($"/public/purchase-orders/{id}");
        var output = await response.Content.ReadFromJsonAsync<SuccessResponse<PurchaseOrderResponse>>(
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output!.Data.Should().BeEquivalentTo(
            new PurchaseOrderResponse(
                Id: id,
                CustomerId: customerId,
                CustomerName: "Arnold Schwarzenegger",
                Items: new[]
                {
                    new PurchaseOrderItemFull(
                        Id: 1,
                        ProductId: productId,
                        ProductName: "A thing to call people",
                        Quantity: 1,
                        Price: 9999)
                },
                TotalPrice: 9999));
    }

    [Fact]
    public async Task Delete_PurchaseOrder()
    {
        // Arrange
        var client = fixure.Factory.CreateClient();
        IPurchaseOrderDbRepository repository = fixure.Factory.Services.GetRequiredService<IPurchaseOrderDbRepository>();
        ICustomerDbRepository customerRepository = fixure.Factory.Services.GetRequiredService<ICustomerDbRepository>();
        IProductDbRepository productDbRepository = fixure.Factory.Services.GetRequiredService<IProductDbRepository>();
        if (await productDbRepository.InsertProductAsync(productDbModel) is not DbSingleRowResult<int>(int productId))
        {
            throw new Exception("Product insertion failed");
        }

        if (await customerRepository.InsertCustomerAsync(customerDbModel) is not DbSingleRowResult<int>(int customerId))
        {
            throw new Exception("Customer insertion failed");
        }
        var toBeInserted = purchaseOrderDbModel with
        {
            Customer = purchaseOrderDbModel.Customer with { Id = customerId },
            Items = [
                purchaseOrderDbModel.Items.First() with { Product = purchaseOrderDbModel.Items.First().Product with { Id = productId } }
            ]
        };
        if (await repository.InsertPurchaseOrderAsync(toBeInserted) is not DbSingleRowResult<int>(int id))
        {
            throw new Exception("Purchase Order insertion failed");
        }

        // Act
        var response = await client.DeleteAsync($"/public/purchase-orders/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var getResult = await repository.GetPurchaseOrderAsync(id);
        getResult.Should().BeOfType<DbEmptyResult<PurchaseOrderModels.PurchaseOrderDbModel>>();
    }

    private PurchaseOrderModels.PurchaseOrderDbModel purchaseOrderDbModel = new(
        Id: null,
        Customer: new(
            Id: 1,
            Name: "Arnold Schwarzenegger",
            Address: "Hollywood Blvd 123",
            ZipCode: "12345",
            City: "Los Angeles",
            Country: "USA"
        ),
        Items: [
            new PurchaseOrderModels.PurchaseOrderItem(
                Id: 1,
                Product: new(
                    Id: 1,
                    Name: "A thing to call people",
                    Price: 9999
                ),
                Quantity: 1)
        ]);

    private ProductModels.ProductDbModel productDbModel = new(
        Id: null,
        ShortName: "Phone",
        Description: "A thing to call people",
        Properties: new Dictionary<string, string>
        {
            { "Color", "Red" },
            { "Size", "Small" }
        },
        Price: 9999);

    private CustomerModels.CustomerDbModel customerDbModel = new(
        Id: null,
        Name: "Arnold Schwarzenegger",
        Address: "Hollywood Blvd 123",
        ZipCode: "12345",
        City: "Los Angeles",
        Country: "USA");
}