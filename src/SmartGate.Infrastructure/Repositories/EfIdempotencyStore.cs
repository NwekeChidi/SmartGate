using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartGate.Application.Abstractions;
using SmartGate.Infrastructure.Database;
using SmartGate.Infrastructure.Database.Setup;

namespace SmartGate.Infrastructure.Repositories;

public class EfIdempotencyStore : IIdempotencyStore
{ 
    private readonly SmartGateDbContext _db;
    private readonly ILogger<EfIdempotencyStore> _log;

    public EfIdempotencyStore(SmartGateDbContext db, ILogger<EfIdempotencyStore> log)
    {
        _db = db;
        _log = log;
    }

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
            _log.LogDebug($"Idempotency reserved {key}");
            return true;
        }
        catch (DbUpdateException)
        {
            _log.LogDebug("Idempotency reservation failed; duplicate key {Key}", key);
            return false;
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
        _log.LogDebug($"Idempotency completed {key} -> Visit {visitId}");
    }
}