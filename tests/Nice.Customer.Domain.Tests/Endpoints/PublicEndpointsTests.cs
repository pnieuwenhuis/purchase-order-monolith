using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Nice.Core.Web;
using Nice.Customer.Domain.Endpoints;
using Nice.Customer.Domain.Services;
using Xunit;

namespace Nice.Customer.Domain.Tests.Endpoints;

public class PublicEndpointsTests
{
    [Fact]
    public async Task GetCustomerById_Should_Return_200OK_When_Found()
    {
        var customerServiceMock = new Mock<ICustomerService>();
        var subject = new PublicEndpoints(customerServiceMock.Object);

        var customerId = 1;
        customerServiceMock.Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(new CustomerService.CustomerFound(TestData.Customer));

        var result = await subject.GetCustomerById(customerId);

        result.Should()
            .BeOfType<Ok<SuccessDetails>>().Which.Value.Should()
            .BeEquivalentTo(
                SuccessDetails.From(new GetCustomerResponse(Id: 1, Name: "John Doe", Address: "123 Main St", ZipCode: "12345", City: "Anytown", Country: "NY")));
        customerServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetCustomerById_Should_Return_404NotFound_When_Not_Found()
    {
        var customerServiceMock = new Mock<ICustomerService>();
        var subject = new PublicEndpoints(customerServiceMock.Object);

        var customerId = 1;
        customerServiceMock.Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(new CustomerService.CustomerNotFound());

        var result = await subject.GetCustomerById(customerId);

        result.Should().BeOfType<NotFound>();
        customerServiceMock.VerifyAll();
    }

    [Fact]
    public async Task InsertCustomerAsync_Should_Return_200OK_When_Inserted()
    {
        var customerServiceMock = new Mock<ICustomerService>();
        var subject = new PublicEndpoints(customerServiceMock.Object);
        var input = new PostCustomerRequest(Id: null, Name: "John Doe", Address: "123 Main St", ZipCode: "12345", City: "Anytown", Country: "NY");

        var customerId = 1;
        CustomerModel customerModel;
        customerServiceMock.Setup(x => x.InsertCustomerAsync(It.IsAny<CustomerModel>()))
            .Callback<CustomerModel>(x => customerModel = x)
            .ReturnsAsync(new CustomerService.CustomerInserted(customerId));

        var result = await subject.InsertCustomerAsync(input);

        result.Should()
            .BeOfType<Ok<SuccessDetails>>().Which.Value.Should()
            .BeEquivalentTo(
                SuccessDetails.From(new PostCustomerResponse(customerId)));
        customerServiceMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteCustomerAsync_Should_Return_200OK_When_Inserted()
    {
        var customerServiceMock = new Mock<ICustomerService>();
        var subject = new PublicEndpoints(customerServiceMock.Object);

        var customerId = 1;
        customerServiceMock.Setup(x => x.DeleteCustomerAsync(customerId))
            .ReturnsAsync(new CustomerService.CustomerDeleted());

        var result = await subject.DeleteCustomerAsync(customerId);

        result.Should()
            .BeOfType<Accepted>();
        customerServiceMock.VerifyAll();
    }
}
