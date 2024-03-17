using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Nice.Postgres.Data;

namespace Nice.PurchaseOrder.Domain.Data;

public interface IPurchaseOrderDbRepository
{
    Task<DbResult<Models.PurchaseOrderDbModel>> GetPurchaseOrderAsync(int id);
    Task<DbResult<int>> InsertPurchaseOrderAsync(Models.PurchaseOrderDbModel purchaseOrder);
    Task<DbResult<int>> DeletePurchaseOrderAsync(int id);
}

[RegisterSingleton<IPurchaseOrderDbRepository>]
public class PurchaseOrderDbRepository(IDbConnection connection, ILogger<DbRepository> logger, IClock clock) : DbRepository(connection, logger), IPurchaseOrderDbRepository
{
    public async Task<DbResult<Models.PurchaseOrderDbModel>> GetPurchaseOrderAsync(int id)
    {
        DbResult<Models.GetPurchaseOrderQueryResult> result = await WithConnectionToSingleRow(conn => conn.QuerySingleOrDefaultAsync<Models.GetPurchaseOrderQueryResult>(
            """SELECT id AS "Id", purchase_order_json AS "PurchaseOrderJson" FROM purchase_order.purchase_order WHERE id = @Id""",
            new { Id = id }));

        return DbResult.SingleRowOrEmptyOrNonSuccess(result, r => (r.Id, r.PurchaseOrderJson.ToDocument()).ToPurchaseOrderDbModel());
    }

    public async Task<DbResult<int>> InsertPurchaseOrderAsync(Models.PurchaseOrderDbModel purchaseOrder) =>
        await WithConnectionToResult(conn => conn.ExecuteScalarAsync<int>(
            @"
                INSERT INTO purchase_order.purchase_order (purchase_order_json, created, updated) 
                VALUES (@PurchaseOrderJson::jsonb, @Now, @Now)
                RETURNING id",
            new
            {
                PurchaseOrderJson = purchaseOrder.ToPurchaseOrderDocument().ToJsonString(),
                clock.Now
            }));

    public async Task<DbResult<int>> DeletePurchaseOrderAsync(int id) =>
        await WithConnectionToResult(conn => conn.ExecuteAsync(
            "DELETE FROM purchase_order.purchase_order WHERE id = @Id",
            new { Id = id }));
}

public static class PurchaseOrderDbRepositoryExtensions
{
    public static string ToJsonString(this Documents.PurchaseOrder customer) => JsonSerializer.Serialize(customer, DataJsonSerializerContext.Default.PurchaseOrder);
    public static Documents.PurchaseOrder ToDocument(this string json) => JsonSerializer.Deserialize(json, DataJsonSerializerContext.Default.PurchaseOrder)!;
}
