using SmartGate.Domain.Visits;

namespace SmartGate.Application.Visits.Dto;

public sealed record DriverDto(
    string FirstName,
    string LastName
);

public sealed record ActivityDto(ActivityType Type, string UnitNumberRaw);

public sealed record CreateVisitRequest(
    string TruckLicensePlate,
    DriverDto Driver,
    IReadOnlyList<ActivityDto> Activities,
    string? IdempotencyKey
);

public sealed record ActivityResponse(
    Guid Id,
    ActivityType Type,
    string UnitNumber
);

public sealed record VisitResponse(
    Guid Id,
    VisitStatus Status,
    string TruckLicensePlate,
    string DriverFirstName,
    string DriverLastName,
    IReadOnlyList<ActivityResponse> Activities,
    string CreatedBy,
    string UpdatedBy,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);

public sealed record UpdateVisitStatusRequest(Guid VisitId, VisitStatus NewStatus);

public sealed record PaginatedResult<T>(int Page, int PageSize, int Count, IReadOnlyList<T> Items);