namespace SmartGate.Application.Abstractions;

// Stores idempotent operation results by a caller-provided key for a short TTL.
public interface IIdempotencyStore
{
    // Task<bool> ExistsAsync(string key, CancellationToken ct);
    // Task RememberAsync(string key, Guid visitId, CancellationToken ct);
    // Task<Guid?> GetVisitIdAsync(string key, CancellationToken ct);
    Task<bool> TryReserveAsync(string key, CancellationToken ct);
    Task CompleteAsync(string key, Guid visitId, CancellationToken ct);
}