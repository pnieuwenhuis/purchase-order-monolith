namespace Nice.Postgres.Data;

/// <summary>
/// Migration marker interface for the database schema migration scripts.
/// </summary>
public interface IDatabaseSchema
{
    string Schema { get; }
}
