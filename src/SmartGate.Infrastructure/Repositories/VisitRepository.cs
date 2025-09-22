using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartGate.Application.Visits.Ports;
using SmartGate.Domain.Visits.Entities;
using SmartGate.Infrastructure.Database;

namespace SmartGate.Infrastructure.Repositories;

public class VisitRepository(SmartGateDbContext db, ILogger<VisitRepository> log) : IVisitRepository
{
    private readonly SmartGateDbContext _db = db;
    private readonly ILogger<VisitRepository> _log = log;

    public async Task AddAsync(Visit visit, CancellationToken ct)
    {
        _log.LogDebug("[EF] Add Visit {VisitId}", visit.Id);
        await _db.Visits.AddAsync(visit, ct);
    }

    public async Task<Visit?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        _log.LogDebug("[EF] GetById {VisitId}", id);
        return await _db.Visits
            .Include(v => v.Driver)
            .Include(v => v.Activities)
            .SingleOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task<IReadOnlyList<Visit>> ListAsync(PageRequest pageRequest, CancellationToken ct)
    {
        var skip = (pageRequest.Page - 1) * pageRequest.PageSize;
        _log.LogDebug("[EF] List page {Page} size {Size}", pageRequest.Page, pageRequest.PageSize);

        return await _db.Visits
        .AsNoTracking()
        .Include(v => v.Driver)
        .Include(v => v.Activities)
        .OrderByDescending(v => v.CreatedAtUTC)
        .Skip(skip)
        .Take(pageRequest.PageSize)
        .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        _log.LogDebug("[EF] SaveChanges");
        return _db.SaveChangesAsync(ct);
    }
}
