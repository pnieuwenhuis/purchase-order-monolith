using System.Net.Http.Json;
using System.Text;
using Xunit;
using System.Text.Json;
using Nice.Customer.Domain.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nice.Postgres.Data;
using Nice.Customer.Domain.Endpoints;
using System.Net;

namespace Nice.PurchaseOrder.IntegationTests;

[Collection("Integration tests")]
public class CustomerTests
{
    private readonly ServiceFixure fixure;

    public CustomerTests(ServiceFixure fixure)
    {
        this.fixure = fixure;
    }

    [Fact]
    public async Task Store_Customer()
    {
        // Arrange
        var client = fixure.Factory.CreateClient();
        ICustomerDbRepository repository = fixure.Factory.Services.GetRequiredService<ICustomerDbRepository>();
        var body = """
        {
            "name": "Arnold Schwarzenegger",
            "address": "Hollywood Blvd 123",
            "zip_code": "12345",
            "city": "Los Angeles",
            "country": "USA"
        }
        """;

        // Act
        var response = await client.PostAsync("/public/customers", new StringContent(body, Encoding.UTF8, "application/json"));
        var output = await response.Content.ReadFromJsonAsync<SuccessResponse<PostCustomerResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResult = await repository.GetCustomerAsync(output!.Data.Id);
        getResult.Should().BeOfType<DbSingleRowResult<Models.CustomerDbModel>>()
            .Which.Row.Should().BeEquivalentTo(customerDbModel with { Id = output.Data.Id });
    }

    [Fact]
    public async Task Get_Customer()
    {
        // Arrange
        var client = fixure.Factory.CreateClient();
        ICustomerDbRepository repository = fixure.Factory.Services.GetRequiredService<ICustomerDbRepository>();
        var insertResult = await repository.InsertCustomerAsync(customerDbModel);
        if (insertResult is not DbSingleRowResult<int>(int id))
        {
            throw new Exception("Insertion failed");
        }

        // Act
        var response = await client.GetAsync($"/public/customers/{id}");
        var output = await response.Content.ReadFromJsonAsync<SuccessResponse<GetCustomerResponse>>(
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output!.Data.Should().BeEquivalentTo(
            new GetCustomerResponse(
                Id: id,
                Name: "Arnold Schwarzenegger",
                Address: "Hollywood Blvd 123",
                ZipCode: "12345",
                City: "Los Angeles",
                Country: "USA"));
    }

    [Fact]
    public async Task Delete_Customer()
    {
        // Arrange
        var client = fixure.Factory.CreateClient();
        ICustomerDbRepository repository = fixure.Factory.Services.GetRequiredService<ICustomerDbRepository>();
        var insertResult = await repository.InsertCustomerAsync(customerDbModel);
        if (insertResult is not DbSingleRowResult<int>(int id))
        {
            throw new Exception("Insertion failed");
        }

        // Act
        var response = await client.DeleteAsync($"/public/customers/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var getResult = await repository.GetCustomerAsync(id);
        getResult.Should().BeOfType<DbEmptyResult<Models.CustomerDbModel>>();
    }

    private Models.CustomerDbModel customerDbModel = new(
        Id: null,
        Name: "Arnold Schwarzenegger",
        Address: "Hollywood Blvd 123",
        ZipCode: "12345",
        City: "Los Angeles",
        Country: "USA");
}