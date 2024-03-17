using Nice.Product.Domain.Services;
using Riok.Mapperly.Abstractions;

namespace Nice.Product.Domain.Endpoints;

[Mapper]
public static partial class EndpointMappers
{
    public static partial ProductModel ToProductModel(this PostProductRequest product);
    public static partial GetProductResponse ToResponse(this ProductModel product);
    public static partial PostProductResponse ToResponse(this ProductService.ProductInserted inserted);
}
