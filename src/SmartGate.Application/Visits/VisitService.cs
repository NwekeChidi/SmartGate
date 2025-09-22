using FluentValidation;
using SmartGate.Application.Abstractions;
using SmartGate.Application.Visits.Dto;
using SmartGate.Application.Visits.Ports;
using SmartGate.Domain.Visits.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace SmartGate.Application.Visits;

public sealed class VisitService : IVisitService
{
    private readonly IVisitRepository _repo;
    private readonly IValidator<CreateVisitRequest> _createValidator;
    private readonly IClock _clock;
    private readonly IPiiPolicy _pii;
    private readonly IIdempotencyStore _idem;
    private readonly IUserContext _user;
    private readonly IDriverRepository _drivers;
    private readonly ILogger<VisitService> _log;
    private readonly IMemoryCache _cache;
    public VisitService(
        IVisitRepository repo,
        IValidator<CreateVisitRequest> createValidator,
        IClock clock,
        IPiiPolicy pii,
        IIdempotencyStore idem,
        IUserContext user,
        IDriverRepository drivers,
        ILogger<VisitService> log,
        IMemoryCache cache)
    {
        _repo = repo;
        _createValidator = createValidator;
        _clock = clock;
        _pii = pii;
        _idem = idem;
        _user = user;
        _drivers = drivers;
        _log = log;
        _cache = cache;
    }

    public async Task<VisitResponse> CreateVisitAsync(CreateVisitRequest request, CancellationToken ct = default)
    {
        _log.LogInformation(
            $"CreateVisit requested by {_user.Subject} for plate {request.TruckLicensePlate} with {request.Activities.Count} activities");
        await _createValidator.ValidateAndThrowAsync(request, ct);

        // Idempotency
        var key = request.IdempotencyKey;
        if (key.HasValue && key.Value != Guid.Empty)
        {
            var reserved = await _idem.TryReserveAsync(key.Value, ct);
            if (!reserved)
                throw new DuplicateRequestException($"A request with IdempotencyKey '{key}' already exists.");
        }

        var normalizedDriverId = request.Driver.Id.ToUpperInvariant();
        var driverEntity = await _drivers.GetByIdAsync(normalizedDriverId, ct);
        if (driverEntity == null)
        {
            driverEntity = new Driver(
                _pii.SanitizeFirstName(request.Driver.FirstName),
                _pii.SanitizeLastName(request.Driver.LastName),
                normalizedDriverId
            );
            await _drivers.AddAsync(driverEntity, ct);
        }

        var truck = new Truck(request.TruckLicensePlate);
        var activities = request.Activities
            .Select(a => new Activity(a.Type, a.UnitNumber))
            .ToList();

        var now = _clock.UtcNow;
        var visit = new Visit(truck, driverEntity, activities, createdBy: _user.Subject, nowUTC: now);

        await _repo.AddAsync(visit, ct);
        await _repo.SaveChangesAsync(ct);

        if (key.HasValue && key.Value != Guid.Empty)
            await _idem.CompleteAsync(key.Value, visit.Id, ct);

        // Invalidate list cache when new visit is created
        InvalidateListCache();

         _log.LogInformation($"Visit {visit.Id} created at {visit.CreatedAtUTC} by {_user.Subject}");
        return Map(visit);
    }

    public async Task<PaginatedResult<VisitResponse>> ListVisitsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        _log.LogDebug($"ListVisits page {page} size {pageSize} by {_user.Subject}");

        if (pageSize > 200) pageSize = 200;

        var pr = (page < 1 || pageSize < 1) ? PageRequest.Default(page, pageSize) : new PageRequest(page, pageSize);
        var cacheKey = $"visits_list_{pr.Page}_{pr.PageSize}";
        
        if (_cache.TryGetValue(cacheKey, out PaginatedResult<VisitResponse>? cached))
        {
            _log.LogDebug($"Cache hit for {cacheKey}");
            return cached!;
        }

        var visits = await _repo.ListAsync(pr, ct);
        var items = visits.Select(Map).ToList();
        var result = new PaginatedResult<VisitResponse>(pr.Page, pr.PageSize, items.Count, items);
        
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
        _log.LogDebug($"Cached result for {cacheKey}");
        
        return result;
    }

    public async Task<VisitResponse> UpdateVisitStatusAsync(UpdateVisitStatusRequest request, Guid VisitId, CancellationToken ct = default)
    {
        var visit = await _repo.GetByIdAsync(VisitId, ct)
            ?? throw new KeyNotFoundException("Visit not found.");

        var from = visit.Status;
        visit.UpdateStatus(request.NewStatus, _user.Subject, _clock.UtcNow);

        await _repo.SaveChangesAsync(ct);
        
        // Invalidate list cache when visit status is updated
        InvalidateListCache();
        
        _log.LogInformation($"Visit {visit.Id} status {from} to {visit.Status} by {_user.Subject} at {visit.UpdatedAtUTC}");
        return Map(visit);
    }

    private static VisitResponse Map(Visit v)
    {
        var driverInfo = new DriverDto(
            FirstName: v.Driver.FirstName,
            LastName: v.Driver.LastName,
            Id: v.Driver.Id
        );

        return new VisitResponse(
            v.Id,
            v.Status,
            TruckLicensePlate: v.Truck.LicensePlateNormalized,
            DriverInformation: driverInfo,
            Activities: [.. v.Activities.Select(a =>
                new ActivityResponse(
                    a.Id,
                    a.Type,
                    UnitNumber: a.UnitNumberNormalized
                ))],
            CreatedAtUtc: v.CreatedAtUTC,
            UpdatedAtUtc: v.UpdatedAtUTC,
            CreatedBy: v.CreatedBy,
            UpdatedBy: v.UpdatedBy
        );
    }
    
    private void InvalidateListCache()
    {
        // Simple approach: remove common cache keys
        var commonKeys = new[]
        {
            "visits_list_1_20", "visits_list_1_50", "visits_list_1_100",
            "visits_list_2_20", "visits_list_2_50", "visits_list_3_20"
        };
        
        foreach (var key in commonKeys)
            _cache.Remove(key);
            
        _log.LogDebug("Invalidated cached list entries");
    }
}
