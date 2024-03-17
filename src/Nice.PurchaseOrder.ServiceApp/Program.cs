Service.Start([
    new Nice.PurchaseOrder.ServiceApp.AppModule(),
    new Nice.PurchaseOrder.Domain.ServiceModule(),
    new Nice.Customer.Domain.ServiceModule(),
    new Nice.Product.Domain.ServiceModule(),
    new Nice.Postgres.Data.ServiceModule()
]);

public partial class Program { }
