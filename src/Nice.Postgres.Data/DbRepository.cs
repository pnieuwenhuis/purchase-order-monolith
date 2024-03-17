using System.Data;

namespace Nice.Postgres.Data;

public abstract partial class DbRepository(IDbConnection connection, ILogger<DbRepository> logger)
{
    /// <summary>
    /// Executes the action with the connection and returns a single row or a simple type result as a typed <see cref="DbResult"/>.
    /// </summary>
    /// <typeparam name="T">The DTO or simple type received from the connection</typeparam>
    /// <param name="action">The action receives an <see cref="IDbConnection"/> object to perform an operation on it. Dapper extension methods should be used here.</param>
    /// <returns>A typed result that maybe contains data or is not successful</returns>
    protected async Task<DbResult<T>> WithConnectionToResult<T>(Func<IDbConnection, Task<T>> action)
    {
        try
        {
            return DbResult.SingleRow(await action(connection));

        }
        catch (Exception ex)
        {
            LogErrorExecutingStatement(logger, ex);
            return DbResult.NonSuccess<T>();
        }
    }

    /// <summary>
    /// Executes the action with the connection and returns a collection of rows as result as a typed <see cref="DbResult"/>.
    /// </summary>
    /// <typeparam name="T">The DTO or simple type received from the connection</typeparam>
    /// <param name="action">The action receives an <see cref="IDbConnection"/> object to perform an operation on it. Dapper extension methods that return collections should be used here.</param>
    /// <returns>A empty or non-empty collection of typed rows result that contains data or is not successful</returns>
    protected async Task<DbResult<T>> WithConnectionToManyResults<T>(Func<IDbConnection, Task<IEnumerable<T>>> action)
    {
        try
        {
            return DbResult.ManyRows((IEnumerable<T>?)await action(connection) ?? Enumerable.Empty<T>());
        }
        catch (Exception ex)
        {
            LogErrorExecutingStatement(logger, ex);
            return DbResult.NonSuccess<T>();
        }
    }

    /// <summary>
    /// Executes the action with the connection and returns a single row or a simple type result as a typed object. Can
    /// be used for transformations to a DTO.
    /// </summary>
    /// <param name="action">The action receives an <see cref="IDbConnection"/> object to perform an operation on it. Dapper extension methods should be used here.</param>
    /// <returns>A typed result that maybe contains data or is not successful</returns>
    /// <exception cref="InvalidOperationException">The connection returned a non-supported result by this method, for example a collection of rows. 
    /// Use 'WithConnectionToManyRows' when you expect a collection of rows.
    /// </exception>
    protected async Task<DbResult<TTyped>> WithConnectionToSingleRow<TTyped>(Func<IDbConnection, Task<TTyped?>> action) =>
        await WithConnectionToResult(action) switch
        {
            DbSingleRowResult<TTyped?>(var data) when data is null => new DbEmptyResult<TTyped>(),
            DbSingleRowResult<TTyped?>(var data) => new DbSingleRowResult<TTyped>(data!),
            DbNonSuccessResult<TTyped?> => new DbNonSuccessResult<TTyped>(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    /// <summary>
    /// Executes the action with the connection and returns a collection of rows as result as a typed object. Can
    /// be used for transformations to a DTO.
    /// </summary>
    /// <param name="action">The action receives an <see cref="IDbConnection"/> object to perform an operation on it. Dapper extension methods that return collections should be used here.</param>
    /// <returns>A empty or non-empty collection of typed object rows result that contains data or is not successful</returns>
    /// <exception cref="InvalidOperationException">The connection returned a non-supported result by this method, for example a single row. 
    /// Use 'WithConnectionToSingleRow' when you expect a single row.
    /// </exception>
    protected async Task<DbResult<TTyped>> WithConnectionToManyRows<TTyped>(Func<IDbConnection, Task<IEnumerable<TTyped>>> action) =>
        await WithConnectionToManyResults(action) switch
        {
            DbManyRowsResult<TTyped>(var data) => new DbManyRowsResult<TTyped>(data),
            DbNonSuccessResult<TTyped?> => new DbNonSuccessResult<TTyped>(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Error while executing SQL statement")]
    internal static partial void LogErrorExecutingStatement(
    ILogger logger,
    Exception ex);
}
