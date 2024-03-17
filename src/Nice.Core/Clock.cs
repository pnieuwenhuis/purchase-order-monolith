using Injectio.Attributes;

namespace Nice.Core;

public interface IClock
{
    DateTimeOffset Now { get; }
}

[RegisterSingleton<IClock>]
public class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}

public class FakeClock(DateTimeOffset now) : IClock
{
    public DateTimeOffset Now => now;
}
