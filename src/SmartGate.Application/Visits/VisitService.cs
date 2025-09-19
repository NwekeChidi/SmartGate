using FluentValidation;
using SmartGate.Application.Abstractions;
using SmartGate.Application.Visits.Dto;
using SmartGate.Application.Visits.Ports;
using SmartGate.Domain.Visits.Entities;
using Microsoft.Extensions.Logging;

namespace SmartGate.Application.Visits;

public sealed class VisitService : IVisitService
{
    private readonly IVisitRepository _repo;
    private readonly IValidator<CreateVisitRequest> _createValidator;
    private readonly IValidator<UpdateVisitStatusRequest> _statusValidator;
    private readonly IClock _clock;
    private readonly IPiiPolicy _pii;
    private readonly IIdempotencyStore _idem;
    private readonly IUserContext _user;
    private readonly ILogger<VisitService> _log;
    public VisitService(
        IVisitRepository repo,
        IValidator<CreateVisitRequest> createValidator,
        IValidator<UpdateVisitStatusRequest> statusValidator,
        IClock clock,
        IPiiPolicy pii,
        IIdempotencyStore idem,
        IUserContext user,
        ILogger<VisitService> log)
    {
        _repo = repo;
        _createValidator = createValidator;
        _statusValidator = statusValidator;
        _clock = clock;
        _pii = pii;
        _idem = idem;
        _user = user;
        _log = log;
    }

    public async Task<VisitResponse> CreateVisitAsync(CreateVisitRequest request, CancellationToken ct = default)
    {
        _log.LogInformation(
            $"CreateVisit requested by {_user.Subject} for plate {request.TruckLicensePlate} with {request.Activities.Count} activities");
        await _createValidator.ValidateAndThrowAsync(request, ct);

        // Idempotency
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            if (await _idem.ExistsAsync(request.IdempotencyKey!, ct))
            {
                var existingId = await _idem.GetVisitIdAsync(request.IdempotencyKey!, ct);
                if (existingId is { } id)
                {
                    var existing = await _repo.GetByIdAsync(id, ct);
                    if (existing is not null)
                    {
                        _log.LogInformation(
                            $"Idempotency hit for key {request.IdempotencyKey}; returning existing Visit {id}");
                        return Map(existing);
                    }
                }
                _log.LogDebug($"Idempotency key {request.IdempotencyKey} exists but visit not found; proceeding to create");
            }
        }

        // PII policy to attempt
        var driver = new Driver(
            _pii.SanitizeFirstName(request.Driver.FirstName),
            _pii.SanitizeLastName(request.Driver.LastName)
        );

        var truck = new Truck(request.TruckLicensePlate);
        var activities = request.Activities
            .Select(a => new Activity(a.Type, a.UnitNumberRaw))
            .ToList();

        var now = _clock.UTCNow;
        var visit = new Visit(truck, driver, activities, nowUTC: now, createdBy: _user.Subject);

        await _repo.AddAsync(visit, ct);
        await _repo.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            await _idem.RememberAsync(request.IdempotencyKey!, visit.Id, ct);

         _log.LogInformation($"Visit {visit.Id} created at {visit.CreatedAtUTC} by {_user.Subject}");
        return Map(visit);
    }

    public async Task<PaginatedResult<VisitListItem>> ListVisitsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        _log.LogDebug($"ListVisits page {page} size {pageSize} by {_user.Subject}");

        if (pageSize > 200) pageSize = 200;

        var pr = (page < 1 || pageSize < 1) ? PageRequest.Default(page, pageSize) : new PageRequest(page, pageSize);
        var items = await _repo.ListAsync(pr, ct);
        return new PaginatedResult<VisitListItem>(pr.Page, pr.PageSize, items.Count, items);
    }

    public async Task<VisitResponse> UpdateVisitStatusAsync(UpdateVisitStatusRequest request, CancellationToken ct = default)
    {
        await _statusValidator.ValidateAndThrowAsync(request, ct);

        var visit = await _repo.GetByIdAsync(request.VisitId, ct)
            ?? throw new KeyNotFoundException("Visit not found.");

        var from = visit.Status;
        visit.UpdateStatus(request.NewStatus, _user.Subject, _clock.UTCNow);

        await _repo.SaveChangesAsync(ct);
        _log.LogInformation($"Visit {visit.Id} status {from} to {visit.Status} by {_user.Subject} at {visit.UpdatedAtUTC}");
        return Map(visit);
    }

    private static VisitResponse Map(Visit v)
    {
        var driverInfo = new DriverInformationDto(
            FirstName: v.Driver.FirstName,
            LastName: v.Driver.LastName,
            Id: v.Driver.Id.ToString()
        );

        return new VisitResponse(
            v.Id,
            v.Status,
            TruckLicensePlate: v.Truck.LicensePlateNormalized,
            DriverInformation: driverInfo,
            Activities: v.Activities.Select(a =>
                new ActivityResponse(
                    a.Id,
                    a.Type,
                    UnitNumber: a.UnitNumberNormalized
                )).ToList(),
            CreatedAtUtc: v.CreatedAtUTC,
            UpdatedAtUtc: v.UpdatedAtUTC,
            CreatedBy: v.CreatedBy,
            UpdatedBy: v.UpdatedBy
        );
    }
}
