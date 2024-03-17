using Nice.Customer.Domain.Services;

namespace Nice.Customer.Domain.Tests;

public static class TestData
{
    public static CustomerModel Customer =>
        new(Id: 1, Name: "John Doe", Address: "123 Main St", ZipCode: "12345", City: "Anytown", Country: "NY");
}