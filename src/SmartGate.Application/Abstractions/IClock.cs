using System.Diagnostics.CodeAnalysis;

namespace SmartGate.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}

[ExcludeFromCodeCoverage]
public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
