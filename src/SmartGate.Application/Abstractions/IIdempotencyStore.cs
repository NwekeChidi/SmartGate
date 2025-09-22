namespace SmartGate.Application.Abstractions;

// Stores idempotent operation results by a caller-provided key for a short TTL.
public interface IIdempotencyStore
{
    Task<bool> TryReserveAsync(Guid key, CancellationToken ct);
    Task CompleteAsync(Guid key, Guid visitId, CancellationToken ct);
}