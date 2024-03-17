namespace Nice.Customer.Domain.Data;

public static class Models
{
    public record GetCustomerQueryResult(int Id, string CustomerJson);

    public record CustomerDbModel(int? Id, string Name, string Address, string ZipCode, string City, string Country);
}

public static class Documents
{
    public record Customer(string Name, string Address, string ZipCode, string City, string Country);
}

[JsonSerializable(typeof(Documents.Customer))]
public partial class DataJsonSerializerContext : JsonSerializerContext;
