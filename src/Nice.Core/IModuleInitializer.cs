namespace Nice.Core;

public interface IModuleInitializer
{
    Task<bool> Run();
}
