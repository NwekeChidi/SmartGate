using SmartGate.Application.Visits.Dto;

namespace SmartGate.Application.Abstractions;

public interface IVisitService
{
    Task<VisitResponse> CreateVisitAsync(CreateVisitRequest request, CancellationToken ct = default);
    Task<PaginatedResult<VisitResponse>> ListVisitsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<VisitResponse> UpdateVisitStatusAsync(UpdateVisitStatusRequest request, CancellationToken ct = default);
}