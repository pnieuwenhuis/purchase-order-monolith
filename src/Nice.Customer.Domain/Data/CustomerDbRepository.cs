using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Nice.Postgres.Data;

namespace Nice.Customer.Domain.Data;

public interface ICustomerDbRepository
{
    Task<DbResult<Models.CustomerDbModel>> GetCustomerAsync(int id);
    Task<DbResult<int>> InsertCustomerAsync(Models.CustomerDbModel customer);
    Task<DbResult<int>> DeleteCustomerAsync(int id);
}

[RegisterSingleton<ICustomerDbRepository>]
public class CustomerDbRepository(IDbConnection connection, ILogger<DbRepository> logger, IClock clock) : DbRepository(connection, logger), ICustomerDbRepository
{
    public async Task<DbResult<Models.CustomerDbModel>> GetCustomerAsync(int id)
    {
        DbResult<Models.GetCustomerQueryResult> result = await WithConnectionToSingleRow(conn => conn.QuerySingleOrDefaultAsync<Models.GetCustomerQueryResult>(
            """SELECT id AS "Id", customer_json AS "CustomerJson" FROM customer.customer WHERE id = @Id""",
            new { Id = id }));

        return DbResult.SingleRowOrEmptyOrNonSuccess(result, r => (r.Id, r.CustomerJson.ToDocument()).ToCustomerDbModel());
    }

    public async Task<DbResult<int>> InsertCustomerAsync(Models.CustomerDbModel customer) =>
        await WithConnectionToResult(conn => conn.ExecuteScalarAsync<int>(
            @"
                INSERT INTO customer.customer (customer_json, created, updated) 
                VALUES (@CustomerJson::jsonb, @Now, @Now)
                RETURNING id",
            new
            {
                CustomerJson = customer.ToCustomerDocument().ToJsonString(),
                clock.Now
            }));

    public async Task<DbResult<int>> DeleteCustomerAsync(int id) =>
        await WithConnectionToResult(conn => conn.ExecuteAsync(
            "DELETE FROM customer.customer WHERE id = @Id",
            new { Id = id }));
}

public static class CustomerDbRepositoryExtensions
{
    public static string ToJsonString(this Documents.Customer customer) => JsonSerializer.Serialize(customer, DataJsonSerializerContext.Default.Customer);
    public static Documents.Customer ToDocument(this string json) => JsonSerializer.Deserialize(json, DataJsonSerializerContext.Default.Customer)!;
}
