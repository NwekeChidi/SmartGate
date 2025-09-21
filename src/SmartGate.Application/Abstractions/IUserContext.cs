using System.Diagnostics.CodeAnalysis;

namespace SmartGate.Application.Abstractions;

public interface IUserContext
{
    string Subject { get; }
}
[ExcludeFromCodeCoverage]
public sealed class SystemUserContext : IUserContext
{
    public string Subject { get; } = "SYSTEM";
}