using Npgsql;

namespace Nice.Postgres.Data;

/// <summary>
/// Interface of a factory for creating <see cref="IDbConnection"/> objects for the super user and the service user.
/// </summary>
public interface ISqlDatasourceFactory
{
    NpgsqlDataSource CreateForSuperUser();
    NpgsqlDataSource CreateForServiceUser();
}
