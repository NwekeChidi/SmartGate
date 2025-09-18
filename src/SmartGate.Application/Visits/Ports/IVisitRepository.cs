using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Application.Visits.Ports;

public sealed record PageRequest(int Page, int PageSize)
{
    public static PageRequest Default(int page = 1, int pageSize = 20) => new(page, pageSize);
}

public sealed record VisitListItem(
    Guid Id,
    VisitStatus Status,
    string LicensePlateNormalized,
    DateTime CreatedAtUtc);

public interface IVisitRepository
{
    Task AddAsync(Visit visit, CancellationToken ct);
    Task<Visit?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<VisitListItem>> ListAsync(PageRequest pageRequest, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}