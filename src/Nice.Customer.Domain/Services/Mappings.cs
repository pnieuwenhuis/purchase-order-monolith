using Nice.Customer.Domain.Data;
using Riok.Mapperly.Abstractions;

namespace Nice.Customer.Domain.Services;

[Mapper]
public static partial class Mappings
{
    public static partial Models.CustomerDbModel ToCustomerDbModel(this CustomerModel customer);
    public static partial CustomerModel ToCustomerModel(this Models.CustomerDbModel model);
}
