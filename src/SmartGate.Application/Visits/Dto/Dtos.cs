using System.Text.Json.Serialization;
using SmartGate.Domain.Visits;

namespace SmartGate.Application.Visits.Dto;

public sealed record DriverDto(
    string FirstName,
    string LastName,
    string Id
);

public sealed record ActivityDto(ActivityType Type, string UnitNumber);

public sealed record CreateVisitRequest(
    string TruckLicensePlate,
    DriverDto Driver,
    IReadOnlyList<ActivityDto> Activities,
    VisitStatus? Status,
    string? IdempotencyKey
);

public sealed record DriverInformationDto(string FirstName, string LastName, string Id);

public sealed record ActivityResponse(
    Guid Id,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] ActivityType Type,
    string UnitNumber
);

public sealed record VisitResponse(
    Guid Id,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] VisitStatus Status,
    string TruckLicensePlate,
    DriverInformationDto DriverInformation,
    IReadOnlyList<ActivityResponse> Activities,
    string CreatedBy,
    string UpdatedBy,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);

public sealed record UpdateVisitStatusRequest(VisitStatus NewStatus);

public sealed record PaginatedResult<T>(int Page, int PageSize, int Count, IReadOnlyList<T> Items);