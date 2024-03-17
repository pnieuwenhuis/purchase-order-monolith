using Nice.Core.Web;
using Nice.Product.Domain.Services;

namespace Nice.Product.Domain.Endpoints;

[RegisterSingleton<IPublicEndpoints>(Duplicate = DuplicateStrategy.Append)]
public class PublicEndpoints(IProductService productService) : IPublicEndpoints
{
    public string RoutePrefix => "/products";

    public void RegisterEndpoints(RouteGroupBuilder builder)
    {
        builder.MapGet("{id}", GetProductById)
            .WithOpenApi()
            .WithDescription("Get a product by identifier and returns the full details of the product")
            .WithDisplayName("Get product by identifier")
            .WithTags("Products");

        builder.MapPost(string.Empty, PostProductAsync)
            .WithOpenApi()
            .WithDescription("Inserts a new product and return the generated identifier")
            .WithDisplayName("Insert product")
            .WithTags("Products");

        builder.MapDelete("{id}", DeleteProductAsync)
            .WithOpenApi()
            .WithDescription("Deletes a product by identifier if it exists")
            .WithDisplayName("Delete product")
            .WithTags("Products");
    }

    public async Task<IResult> GetProductById(int id) =>
        await productService.GetProductAsync(id) switch
        {
            ProductService.ProductFound(var product) => Results.Ok(SuccessDetails.From(product.ToResponse())),
            ProductService.ProductNotFound => Results.NotFound(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IResult> PostProductAsync(PostProductRequest product) =>
        await productService.InsertProductAsync(product.ToProductModel()) switch
        {
            ProductService.ProductInserted i => Results.Ok(SuccessDetails.From(i.ToResponse())),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    public async Task<IResult> DeleteProductAsync(int id) =>
        await productService.DeleteProductAsync(id) switch
        {
            ProductService.ProductDeleted => Results.Accepted(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };
}
