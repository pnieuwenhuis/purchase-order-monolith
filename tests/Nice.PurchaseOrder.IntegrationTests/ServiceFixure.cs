using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Xunit;

namespace Nice.PurchaseOrder.IntegationTests;

public class ServiceFixure : IAsyncDisposable
{
    public PostgreSqlContainer PostgresContainer { get; init; }
    public WebApplicationFactory<Program> Factory { get; init; }
    public NpgsqlDataSource DataSource { get; init; }

    public ServiceFixure()
    {
        DotNetEnv.Env.Load(".env.integration");

        var postgresPort = 5432;
        PostgresContainer = new PostgreSqlBuilder()
             .WithImage("postgres:16-alpine")
             .WithPortBinding(postgresPort, true)
             .Build();
        PostgresContainer.StartAsync().Wait();

        Environment.SetEnvironmentVariable("PostgresDatabase__Port", PostgresContainer.GetMappedPublicPort(postgresPort).ToString());

        Factory = new WebApplicationFactory<Program>();
        var connectionString = new NpgsqlConnectionStringBuilder(PostgresContainer.GetConnectionString())
        {
            Username = Environment.GetEnvironmentVariable("PostgresDatabase__ServiceUserCredentials__UserName"),
            Password = Environment.GetEnvironmentVariable("PostgresDatabase__ServiceUserCredentials__Password"),
            Database = Environment.GetEnvironmentVariable("PostgresDatabase__DatabaseName")
        };
        DataSource = NpgsqlDataSource.Create(connectionString.ToString());
    }

    public async ValueTask DisposeAsync()
    {
        await PostgresContainer.DisposeAsync();
    }
}

[CollectionDefinition("Integration tests")]
public class ServiceCollection : ICollectionFixture<ServiceFixure>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}