using SmartGate.Application.Visits.Dto;
using SmartGate.Application.Visits.Ports;

namespace SmartGate.Application.Abstractions;

public interface IVisitService
{
    Task<VisitResponse> CreateVisitAsync(CreateVisitRequest request, CancellationToken ct = default);
    Task<PaginatedResult<VisitListItem>> ListVisitsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<VisitResponse> UpdateVisitStatusAsync(UpdateVisitStatusRequest request, CancellationToken ct = default);
}