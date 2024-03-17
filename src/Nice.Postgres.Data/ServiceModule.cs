global using Injectio.Attributes;
global using Nice.Core;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using EvolveDb;
using EvolveDb.Configuration;
using Npgsql;
using System.Data.Common;

[module: DapperAot]

namespace Nice.Postgres.Data;

public class ServiceModule : IAppModule
{
    public void Configure(ModuleConfigurator configurator)
    {
        configurator.RegisterSettingsWithValidator<PostgresDatabaseSettings, PostgresDatabaseSettingsValidator>(
            "PostgresDatabase", section => section.Get<PostgresDatabaseSettings>());
        configurator.Register(services => services.AddDatabaseServices());
        configurator.Register(services => services.AddSingleton(provider =>
            provider.GetRequiredService<ISqlDatasourceFactory>().CreateForServiceUser()));
        configurator.Register(services => services.AddSingleton<IDbConnection>(provider =>
            provider.GetRequiredService<NpgsqlDataSource>().CreateConnection()));
        configurator.SetInitializer<Initializer>();
        configurator.RegisterHealhCheck(builder => builder.AddNpgSql());
    }
}

[RegisterSingleton<Initializer>]
public partial class Initializer(
    ISqlDatasourceFactory datasourceFactory,
    ILogger<Initializer> logger,
    PostgresDatabaseSettings settings,
    IEnumerable<IDatabaseSchema> databaseSchemas) : IModuleInitializer
{
    public async Task<bool> Run()
    {
        LogStartDatabaseInitialisation(logger, settings.HostName!);
        var superUserDatasource = datasourceFactory.CreateForSuperUser();
        if (!await CreateDatabase(superUserDatasource))
        {
            return false;
        }

        var serviceUserDatasource = datasourceFactory.CreateForServiceUser();
        if (!await CreateSchemas(serviceUserDatasource))
        {
            return false;
        }

        return true;
    }

    private async Task<bool> CreateDatabase(DbDataSource datasource)
    {
        IDbConnection? connection = null;
        try
        {
            connection = await datasource.OpenConnectionAsync();

            // create the database user under which the service will execute queries
            if (await connection.ExecuteScalarAsync($"SELECT 1 FROM pg_user WHERE usename = '{settings.ServiceUserCredentials.UserName}'") == null)
            {
                await connection.ExecuteAsync($"CREATE USER \"{settings.ServiceUserCredentials.UserName}\" WITH CREATEDB PASSWORD '{settings.ServiceUserCredentials.Password}'");
            }

            // create the database that this service will own
            if (await connection.ExecuteScalarAsync($"SELECT 1 FROM pg_database WHERE datname = '{settings.DatabaseName}'") == null)
            {
                await connection.ExecuteAsync($"CREATE DATABASE \"{settings.DatabaseName}\" ENCODING 'UTF8' OWNER \"{settings.ServiceUserCredentials.UserName}\"");
            }

            return true;
        }
        catch (Exception ex)
        {
            LogFailedToCreateDatabase(logger, settings.DatabaseName!, settings.HostName!, ex);
            return false;
        }
        finally
        {
            connection?.Close();
        }
    }

    private async Task<bool> CreateSchemas(DbDataSource datasource)
    {
        IDbConnection? connection = null;
        try
        {
            connection = await datasource.OpenConnectionAsync();
            foreach (IDatabaseSchema schema in databaseSchemas)
            {
                LogCreateUpdateDataSchema(logger, settings.DatabaseName!, schema.Schema);
                Type schemaType = schema.GetType();
                var evolve = new Evolve((DbConnection)connection, msg => LogEvolve(logger, msg))
                {
                    EmbeddedResourceAssemblies = [schemaType.Assembly],
                    EmbeddedResourceFilters = [schemaType.Namespace ?? string.Empty],
                    IsEraseDisabled = true,
                    Schemas = new[] { schema.Schema },
                    TransactionMode = TransactionKind.CommitEach,
                    EnableClusterMode = true,
                    Placeholders = new Dictionary<string, string>
                    {
                        ["{schema}"] = schema.Schema,
                        ["{database}"] = settings.DatabaseName!,
                        ["{user}"] = settings.ServiceUserCredentials.UserName!,
                    }
                };

                evolve.Migrate();
            }

            return true;
        }
        catch (Exception ex)
        {
            LogFailedToCreateSchemas(logger, settings.DatabaseName!, ex);
            return false;
        }
        finally
        {
            connection?.Close();
        }
    }

    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = "Failed to create the database '{database}' on host '{hostName}'")]
    internal static partial void LogFailedToCreateDatabase(
        ILogger logger,
        string database,
        string hostName,
        Exception ex);

    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = "Failed to create or update database schemas for '{database}'")]
    internal static partial void LogFailedToCreateSchemas(
        ILogger logger,
        string database,
        Exception ex);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Performing creation or updating operation on '{database}' for schema '{schema}'")]
    internal static partial void LogCreateUpdateDataSchema(
        ILogger logger,
        string database,
        string schema);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Evolve: '{message}'")]
    internal static partial void LogEvolve(
        ILogger logger,
        string message);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Initializing PostgreSQL database at '{hostName}'")]
    internal static partial void LogStartDatabaseInitialisation(
        ILogger logger,
        string hostName);
}
