using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Nice.Postgres.Data;

namespace Nice.Product.Domain.Data;

public interface IProductDbRepository
{
    Task<DbResult<Models.ProductDbModel>> GetProductAsync(int id);
    Task<DbResult<Models.ProductDbModel>> GetProductsAsync(int[] ids);
    Task<DbResult<int>> InsertProductAsync(Models.ProductDbModel product);
    Task<DbResult<int>> DeleteProductAsync(int id);
}

[RegisterSingleton<IProductDbRepository>]
public class ProductDbRepository(IDbConnection connection, ILogger<DbRepository> logger, IClock clock) : DbRepository(connection, logger), IProductDbRepository
{
    public async Task<DbResult<Models.ProductDbModel>> GetProductAsync(int id)
    {
        DbResult<Models.GetProductQueryResult> result = await WithConnectionToSingleRow(conn => conn.QuerySingleOrDefaultAsync<Models.GetProductQueryResult>(
            """SELECT id AS "Id", product_json AS "ProductJson" FROM Product.Product WHERE id = @Id""",
            new { Id = id }));

        return DbResult.SingleRowOrEmptyOrNonSuccess(result, r => (r.Id, r.ProductJson.ToDocument()).ToProductDbModel());
    }

    public async Task<DbResult<Models.ProductDbModel>> GetProductsAsync(int[] ids)
    {
        DbResult<Models.GetProductQueryResult> result = await WithConnectionToManyRows(conn => conn.QueryAsync<Models.GetProductQueryResult>(
            """SELECT id AS "Id", product_json AS "ProductJson" FROM Product.Product WHERE id = ANY(@Ids)""",
            new { Ids = ids }));

        return DbResult.ManyRowsOrNonSuccess(result, r => (r.Id, r.ProductJson.ToDocument()).ToProductDbModel());
    }

    public async Task<DbResult<int>> InsertProductAsync(Models.ProductDbModel product) =>
        await WithConnectionToResult(conn => conn.ExecuteScalarAsync<int>(
            @"
                INSERT INTO Product.Product (product_json, created, updated) 
                VALUES (@ProductJson::jsonb, @Now, @Now)
                RETURNING id",
            new
            {
                ProductJson = product.ToProductDocument().ToJsonString(),
                clock.Now
            }));

    public async Task<DbResult<int>> DeleteProductAsync(int id) =>
        await WithConnectionToResult(conn => conn.ExecuteAsync(
            "DELETE FROM Product.Product WHERE id = @Id",
            new { Id = id }));
}

public static class ProductDbRepositoryExtensions
{
    public static string ToJsonString(this Documents.Product customer) => JsonSerializer.Serialize(customer, DataJsonSerializerContext.Default.Product);
    public static Documents.Product ToDocument(this string json) => JsonSerializer.Deserialize(json, DataJsonSerializerContext.Default.Product)!;
}
