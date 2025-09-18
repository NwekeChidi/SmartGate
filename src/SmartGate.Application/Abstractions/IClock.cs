namespace SmartGate.Application.Abstractions;

public interface IClock
{
    DateTime UTCNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UTCNow => DateTime.UtcNow;
}
