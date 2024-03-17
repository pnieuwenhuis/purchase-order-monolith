using Npgsql;

namespace Nice.Postgres.Data;

/// <summary>
/// Implementation of a factory for creating <see cref="DbDatasource"/> objects for the super user and the service user. Based on
/// the Npgsql library.
/// </summary>
/// <param name="settings">The settings used to initialize the datasource</param>
[RegisterSingleton<ISqlDatasourceFactory>]
public class SqlDatasourceFactory(PostgresDatabaseSettings settings, ILoggerFactory loggerFactory) : ISqlDatasourceFactory
{
    public NpgsqlDataSource CreateForSuperUser()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = settings.HostName,
            Port = settings.Port,
            Username = settings.SuperUserCredentials!.UserName,
            Password = settings.SuperUserCredentials.Password,
            Database = "postgres",
            SslMode = SslMode.Prefer
        };

        return new NpgsqlSlimDataSourceBuilder(builder.ToString())
            .UseLoggerFactory(loggerFactory)
            .Build();
    }

    public NpgsqlDataSource CreateForServiceUser()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = settings.HostName,
            Port = settings.Port,
            Username = settings.ServiceUserCredentials!.UserName,
            Password = settings.ServiceUserCredentials.Password,
            Database = settings.DatabaseName,
            SslMode = SslMode.Prefer
        };

        return new NpgsqlSlimDataSourceBuilder(builder.ToString())
            .UseLoggerFactory(loggerFactory)
            .EnableArrays()
            .EnableRanges()
            .Build();
    }
}

