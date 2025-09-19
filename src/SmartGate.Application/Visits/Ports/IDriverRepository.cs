using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Application.Visits.Ports;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(string id, CancellationToken ct);
    Task AddAsync(Driver driver, CancellationToken ct);
}
