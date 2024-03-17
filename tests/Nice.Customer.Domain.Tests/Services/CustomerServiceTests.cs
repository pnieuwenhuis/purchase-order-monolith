using FluentAssertions;
using Moq;
using Nice.Core.Exceptions;
using Nice.Customer.Domain.Data;
using Nice.Customer.Domain.Services;
using Nice.Postgres.Data;
using Xunit;

namespace Nice.Customer.Domain.Tests.Services;

public class CustomerServiceTests
{
    [Fact]
    public async Task InsertCustomerAsync_Should_Return_CustomerInserted_When_Success()
    {
        var customerDbRepositoryMock = new Mock<ICustomerDbRepository>();
        var subject = new CustomerService(customerDbRepositoryMock.Object);

        Models.CustomerDbModel? customerDbModel = null;
        customerDbRepositoryMock.Setup(x => x.InsertCustomerAsync(It.IsAny<Models.CustomerDbModel>()))
            .Callback<Models.CustomerDbModel>(c => customerDbModel = c)
            .ReturnsAsync(DbResult.SingleRow(1));

        var result = await subject.InsertCustomerAsync(TestData.Customer);

        result.Should()
            .BeOfType<CustomerService.CustomerInserted>().And
            .BeEquivalentTo(new CustomerService.CustomerInserted(1));
        customerDbRepositoryMock.VerifyAll();
        customerDbModel.Should().BeEquivalentTo(
            new Models.CustomerDbModel(1, "John Doe", "123 Main St", "12345", "Anytown", "NY"));
    }

    [Fact]
    public async Task InsertCustomerAsync_Should_Return_DbNonSuccessResult_When_Failed()
    {
        var customerDbRepositoryMock = new Mock<ICustomerDbRepository>();
        var subject = new CustomerService(customerDbRepositoryMock.Object);

        customerDbRepositoryMock.Setup(x => x.InsertCustomerAsync(It.IsAny<Models.CustomerDbModel>()))
            .ReturnsAsync(DbResult.NonSuccess<int>());

        var result = await subject.InsertCustomerAsync(TestData.Customer);

        result.Should().BeOfType<CustomerService.OperationFailure>();
        customerDbRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetCustomerAsync_Should_Return_CustomerFound_When_Found()
    {
        var customerDbRepositoryMock = new Mock<ICustomerDbRepository>();
        var subject = new CustomerService(customerDbRepositoryMock.Object);

        int customerId = 1;
        customerDbRepositoryMock.Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(DbResult.SingleRow(new Models.CustomerDbModel(1, "John Doe", "123 Main St", "12345", "Anytown", "NY")));

        var result = await subject.GetCustomerAsync(customerId);

        result.Should()
            .BeOfType<CustomerService.CustomerFound>().And
            .BeEquivalentTo(new CustomerService.CustomerFound(TestData.Customer));
        customerDbRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetCustomerAsync_Should_Return_CustomerNotFound_When_Not_Found()
    {
        var customerDbRepositoryMock = new Mock<ICustomerDbRepository>();
        var subject = new CustomerService(customerDbRepositoryMock.Object);

        int customerId = 1;
        customerDbRepositoryMock.Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(DbResult.Empty<Models.CustomerDbModel>());

        var result = await subject.GetCustomerAsync(customerId);

        result.Should()
            .BeOfType<CustomerService.CustomerNotFound>();
        customerDbRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetCustomerAsync_Should_Throw_ServiceException_When_Non_Success()
    {
        var customerDbRepositoryMock = new Mock<ICustomerDbRepository>();
        var subject = new CustomerService(customerDbRepositoryMock.Object);

        int customerId = 1;
        customerDbRepositoryMock.Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(DbResult.NonSuccess<Models.CustomerDbModel>());

        var result = await subject.GetCustomerAsync(customerId);

        result.Should().BeOfType<CustomerService.OperationFailure>();
        customerDbRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteCustomerAsync_Should_Return_CustomerDeleted_When_Deleted()
    {
        var customerDbRepositoryMock = new Mock<ICustomerDbRepository>();
        var subject = new CustomerService(customerDbRepositoryMock.Object);

        int customerId = 1;
        customerDbRepositoryMock.Setup(x => x.DeleteCustomerAsync(customerId))
            .ReturnsAsync(DbResult.SingleRow(1));

        var result = await subject.DeleteCustomerAsync(customerId);

        result.Should()
            .BeOfType<CustomerService.CustomerDeleted>();
        customerDbRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task DeleteCustomerAsync_Should_Throw_ServiceException_When_Non_Success()
    {
        var customerDbRepositoryMock = new Mock<ICustomerDbRepository>();
        var subject = new CustomerService(customerDbRepositoryMock.Object);

        int customerId = 1;
        customerDbRepositoryMock.Setup(x => x.DeleteCustomerAsync(customerId))
            .ReturnsAsync(DbResult.NonSuccess<int>());

        var result = await subject.DeleteCustomerAsync(customerId);

        result.Should().BeOfType<CustomerService.OperationFailure>();
        customerDbRepositoryMock.VerifyAll();
    }
}
