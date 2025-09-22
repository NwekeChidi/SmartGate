using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartGate.Application.Visits.Ports;
using SmartGate.Domain.Visits.Entities;
using SmartGate.Infrastructure.Database;

namespace SmartGate.Infrastructure.Repositories;

public sealed class DriverRepository(SmartGateDbContext db, ILogger<DriverRepository> log) : IDriverRepository
{
    private readonly SmartGateDbContext _db = db;
    private readonly ILogger<DriverRepository> _log = log;

    public async Task<Driver?> GetByIdAsync(string id, CancellationToken ct)
    {
        _log.LogDebug("[EF] Get Driver {DriverId}", id);
        return await _db.Drivers.SingleOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task AddAsync(Driver driver, CancellationToken ct)
    {
        _log.LogDebug("[EF] Add Driver {DriverId}", driver.Id);
        await _db.Drivers.AddAsync(driver, ct);
    }
}
