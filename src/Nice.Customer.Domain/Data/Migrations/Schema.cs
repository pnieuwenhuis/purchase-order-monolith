using Nice.Postgres.Data;

namespace Nice.Customer.Domain.Data.Migrations;

[RegisterSingleton<IDatabaseSchema>(Duplicate = DuplicateStrategy.Append)]
public class DatabaseSchema : IDatabaseSchema
{
    public string Schema => "customer";
}
