using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartGate.Application.Abstractions;
using SmartGate.Infrastructure.Database;
using SmartGate.Infrastructure.Database.Setup;

namespace SmartGate.Infrastructure.Repositories;

public class IdempotencyStore(SmartGateDbContext db, ILogger<IdempotencyStore> log) : IIdempotencyStore
{ 
    private readonly SmartGateDbContext _db = db;
    private readonly ILogger<IdempotencyStore> _log = log;

    public async Task<bool> TryReserveAsync(Guid key, CancellationToken ct)
    {
        try
        {
            _db.IdempotencyKeys.Add(new IdempotencyKey
            {
                Key = key,
                VisitId = Guid.Empty,
                CreatedAtUTC = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
            _log.LogDebug($"[EF] Idempotency reserved {key}");
            return true;
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message?.Contains("duplicate key") == true || 
                ex.InnerException?.Message?.Contains("unique constraint") == true)
            {
                _log.LogDebug($"[EF] Idempotency reservation failed; duplicate key {key}");
                return false;
            }
            throw;
        }
    }

    public async Task CompleteAsync(Guid key, Guid visitId, CancellationToken ct)
    {
        var row = await _db.IdempotencyKeys.FirstOrDefaultAsync(x => x.Key == key, ct);
        if (row is null)
        {
            _db.IdempotencyKeys.Add(new IdempotencyKey
            {
                Key = key,
                VisitId = visitId,
                CreatedAtUTC = DateTime.UtcNow
            });
        }
        else
        {
            row.VisitId = visitId;
        }
        await _db.SaveChangesAsync(ct);
        _log.LogDebug($"[EF] Idempotency completed {key} -> Visit {visitId}");
    }
}