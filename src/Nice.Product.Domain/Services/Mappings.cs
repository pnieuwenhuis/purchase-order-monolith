using Nice.Product.Domain.Data;
using Riok.Mapperly.Abstractions;

namespace Nice.Product.Domain.Services;

[Mapper]
public static partial class Mappings
{
    public static partial Models.ProductDbModel ToProductDbModel(this ProductModel product);
    public static partial ProductModel ToProductModel(this Models.ProductDbModel product);
}
