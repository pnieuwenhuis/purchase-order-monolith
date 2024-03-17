using Riok.Mapperly.Abstractions;

namespace Nice.Customer.Domain.Data;

[Mapper]
public static partial class DataMappers
{
    public static partial Documents.Customer ToCustomerDocument(this Models.CustomerDbModel customer);
    public static Models.CustomerDbModel ToCustomerDbModel(this (int Id, Documents.Customer Customer) t) =>
        new(t.Id, t.Customer.Name, t.Customer.Address, t.Customer.ZipCode, t.Customer.City, t.Customer.Country);
}
