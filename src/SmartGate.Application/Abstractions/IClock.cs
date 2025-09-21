using System.Diagnostics.CodeAnalysis;

namespace SmartGate.Application.Abstractions;

public interface IClock
{
    DateTime UTCNow { get; }
}

[ExcludeFromCodeCoverage]
public sealed class SystemClock : IClock
{
    public DateTime UTCNow => DateTime.UtcNow;
}
