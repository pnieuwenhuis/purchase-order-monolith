using Nice.Postgres.Data;

namespace Nice.Product.Domain.Data.Migrations;

[RegisterSingleton<IDatabaseSchema>(Duplicate = DuplicateStrategy.Append)]
public class DatabaseSchema : IDatabaseSchema
{
    public string Schema => "product";
}
