using Riok.Mapperly.Abstractions;

namespace Nice.Product.Domain.Data;

[Mapper]
public static partial class DataMappers
{
    public static partial Documents.Product ToProductDocument(this Models.ProductDbModel customer);
    public static Models.ProductDbModel ToProductDbModel(this (int Id, Documents.Product Product) t) =>
        new(t.Id, t.Product.ShortName, t.Product.Description, t.Product.Properties, t.Product.Price);
}
