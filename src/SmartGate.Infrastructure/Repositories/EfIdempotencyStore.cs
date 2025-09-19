using Microsoft.EntityFrameworkCore;
using SmartGate.Application.Abstractions;
using SmartGate.Infrastructure.Database;
using SmartGate.Infrastructure.Database.Setup;

namespace SmartGate.Infrastructure.Repositories;

public class EfIdempotencyStore : IIdempotencyStore
{ 
    private readonly SmartGateDbContext _db;
    public EfIdempotencyStore(SmartGateDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(string key, CancellationToken ct)
        => await _db.IdempotencyKeys.AnyAsync(x => x.Key == key, ct);

    public async Task RememberAsync(string key, Guid visitId, CancellationToken ct)
    {
        _db.IdempotencyKeys.Add(new IdempotencyKey
        {
            Key = key,
            VisitId = visitId,
            CreatedAtUTC = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Guid?> GetVisitIdAsync(string key, CancellationToken ct)
        => await _db.IdempotencyKeys
            .Where(x => x.Key == key)
            .Select(x => (Guid?)x.VisitId)
            .FirstOrDefaultAsync(ct);
}