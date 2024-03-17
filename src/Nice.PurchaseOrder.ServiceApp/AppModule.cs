global using Injectio.Attributes;
global using Nice.Core;

namespace Nice.PurchaseOrder.ServiceApp;

public class AppModule : IAppModule
{
    public void Configure(ModuleConfigurator configurator)
    {
        configurator.Register(services => services.AddAppServices());
    }
}
