namespace SmartGate.Application.Abstractions;

public sealed class DuplicateRequestException : Exception
{
    public DuplicateRequestException() { }
    public DuplicateRequestException(string message) : base(message) { }
    public DuplicateRequestException(string message, Exception innerException) : base(message, innerException) { }
}
