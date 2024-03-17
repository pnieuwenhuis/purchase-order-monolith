using System.Diagnostics.CodeAnalysis;

namespace Nice.Postgres.Data;

public static class DbResult
{
    /// <summary>
    /// This is returned from the repository when a non-successful result is received from the database. This
    /// is a failure and indicates that the operation could be retried.
    /// </summary>
    /// <typeparam name="T">The type of the database result that is returned</typeparam>
    public static DbResult<T> NonSuccess<T>() => new DbNonSuccessResult<T>();

    /// <summary>
    /// This is returned from the repository when a single row is returned from the database. This is a success
    /// result and the data element contains the columns from the database.
    /// </summary>
    /// <typeparam name="T">Mostly a DTO used in the transfer between the repository and the service layer</typeparam>
    /// <param name="row">The typed row that is returned</param>
    public static DbResult<T> SingleRow<T>(T row) => new DbSingleRowResult<T>(row);

    /// <summary>
    /// This is returned from the repository when multiple rows are returned from the database. This is a success
    /// result and the data element contains the rows with for each row the columns from the database.
    /// </summary>
    /// <typeparam name="T">Mostly a DTO used in the transfer between the repository and the service layer</typeparam>
    /// <param name="rows">A iterable collection of rows</param>
    /// <returns></returns>
    public static DbResult<T> ManyRows<T>(IEnumerable<T> rows) => new DbManyRowsResult<T>(rows);

    /// <summary>
    /// This is returned from the repository when no rows are returned from the database. This is a success
    /// result, but query returned no rows.
    /// </summary>
    /// <typeparam name="T"a DTO used in the transfer between the repository and the service layer when data was found</typeparam>
    /// <returns></returns>
    public static DbResult<T> Empty<T>() => new DbEmptyResult<T>();

    /// <summary>
    /// Evaluates the result returned from a <see cref="IDbConnection"> and returns the single row if the result is a success. It also returns <see cref="DbNonSuccesResult"> if the
    /// result is a non-success result or <see cref="DbEmptyResult"> if the result is empty. Transform an incoming typed row into the typed row that is returned.
    /// </summary>
    /// <typeparam name="T">Mostly a DTO used in the transfer between the repository and the service layer</typeparam>
    /// <typeparam name="TIn">Incoming typed row from the database query result</typeparam>
    /// <param name="result">The result from the connection which is a typed object
    /// <param name="processor">A function to transform the incoming data that is typed query result to typed DTO</param>
    /// <returns>A typed <see cref="DbResult"/> c</returns>
    /// <exception cref="InvalidOperationException">The connection returned a non-supported result by this method, for example a collection of rows. 
    /// Use 'ManyRowsOrNonSuccess' when you expect a collection of rows.
    /// </exception>
    public static DbResult<T> SingleRowOrEmptyOrNonSuccess<T, TIn>(DbResult<TIn> result, Func<TIn, T> processor) =>
        result switch
        {
            DbSingleRowResult<TIn>(var row) => SingleRow(processor(row)),
            DbNonSuccessResult<TIn> => NonSuccess<T>(),
            DbEmptyResult<TIn> => Empty<T>(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };

    /// <summary>
    /// Evaluates the result returned from a <see cref="IDbConnection"> and returns a collection of rows if the result is a success. It also returns <see cref="DbDynamicNonSuccess"> if the
    /// result is a non-success result.
    /// </summary>
    /// <typeparam name="T">Mostly a DTO used in the transfer between the repository and the service layer</typeparam>
    /// <param name="result">The result from the connection which is a typed as <see cref="dynamic"/></param>
    /// <param name="processor">A function to transform the incoming data that is of type <see cref="dynamic"/> to a typed DTO</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">The connection returned a non-supported result by this method, for example a single row. 
    /// Use 'SingleRowOrEmptyOrNonSuccess' when you expect a single row.
    /// </exception>
    /// <remarks>
    /// When the connection doesn't return any record, it returns an empty collection and not a <see cref="DbEmptyResult"/>
    /// </remarks>
    public static DbResult<T> ManyRowsOrNonSuccess<T, TIn>(DbResult<TIn> result, Func<TIn, T> processor) =>
        result switch
        {
            DbManyRowsResult<TIn>(var rows) => ManyRows(rows.Select(processor)),
            DbNonSuccessResult<TIn> => NonSuccess<T>(),
            var unknown => throw new NotImplementedException(unknown.GetType().Name)
        };
}

/// <summary>
/// This is the base class for all the results returned from the repository. It is an abstract class and can't be instantiated.
/// </summary>
/// <typeparam name="T">Mostly a DTO used in the transfer between the repository and the service layer</typeparam>
/// <param name="Success">Indicates that the operation is a success</param>
public abstract record DbResult<T>(bool Success);

/// <summary>
/// This is returned from the repository when a non-successful result is received from the database. This
/// is a failure and indicates that the operation could be retried.
/// </summary>
/// <typeparam name="T">Mostly a DTO used in the transfer between the repository and the service layer in the situation when a record was found</typeparam>
public record DbNonSuccessResult<T>() : DbResult<T>(false);

/// <summary>
/// This is returned from the repository when a single row is returned from the database. This is a success
/// result and the data element contains the columns from the database.
/// </summary>
/// <typeparam name="T">Mostly a DTO used in the transfer between the repository and the service layer in the situation when a record was found</typeparam>
/// <param name="Row">The single row with columns containing the data</param>
public record DbSingleRowResult<T>(T Row) : DbResult<T>(true);

/// <summary>
/// This is returned from the repository when multiple rows are returned from the database. This is a success
/// result and the data element contains the rows with for each row the columns from the database.
/// </summary>
/// <typeparam name="T">Mostly a DTO used in the transfer between the repository and the service layer</typeparam>
/// <param name="Rows">A iterable collection of rows</param>
public record DbManyRowsResult<T>(IEnumerable<T> Rows) : DbResult<T>(true);

/// <summary>
/// This is returned from the repository when no rows are returned from the database. This is a success
/// result, but query returned no rows.
/// </summary>
/// <typeparam name="T">Mostly a DTO used in the transfer between the repository and the service layer in the situation when a record was found</typeparam>
public record DbEmptyResult<T>() : DbResult<T>(true);
