namespace SmartGate.Application.Abstractions;

public sealed class DuplicateRequestException(string message) : Exception(message)
{
}
