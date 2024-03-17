using Nice.Postgres.Data;

namespace Nice.PurchaseOrder.Domain.Data.Migrations;

[RegisterSingleton<IDatabaseSchema>(Duplicate = DuplicateStrategy.Append)]
public class DatabaseSchema : IDatabaseSchema
{
    public string Schema => "purchase_order";
}
