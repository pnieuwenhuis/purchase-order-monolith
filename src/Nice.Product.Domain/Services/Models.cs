namespace Nice.Product.Domain.Services;

public record ProductModel(int? Id, string ShortName, string Description, IDictionary<string, string> Properties, int Price);
