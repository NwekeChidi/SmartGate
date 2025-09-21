using System.Diagnostics.CodeAnalysis;

namespace SmartGate.Application.Abstractions;

public interface IUserContext
{
    string Subject { get; }
}
[ExcludeFromCodeCoverage]
public sealed class SystemUserContext : IUserContext
{
    private const string SystemSubject = "SYSTEM";
    public string Subject { get; } = SystemSubject;
}