namespace Nice.Product.Domain.Data;

public static class Models
{
    public record ProductDbModel(int? Id, string ShortName, string Description, IDictionary<string, string> Properties, int Price);

    public record GetProductQueryResult(int Id, string ProductJson);
}

public static class Documents
{
    public record Product(string ShortName, string Description, IDictionary<string, string> Properties, int Price);
}

[JsonSerializable(typeof(Documents.Product))]
public partial class DataJsonSerializerContext : JsonSerializerContext;

