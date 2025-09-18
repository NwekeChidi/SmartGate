namespace SmartGate.Application.Abstractions;

public interface IUserContext
{
    string Subject { get; }
}

public sealed class SystemUserContext : IUserContext
{
    public string Subject { get; } = "SYSTEM";
}