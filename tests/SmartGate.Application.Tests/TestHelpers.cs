using NSubstitute;
using SmartGate.Application.Abstractions;
using SmartGate.Application.Visits;
using SmartGate.Application.Visits.Dto;
using SmartGate.Application.Visits.Ports;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SmartGate.Application.Visits.Validators;
using Microsoft.Extensions.Caching.Memory;

namespace SmartGate.Application.Tests;

public static class TestHelpers
{
    public static IClock FixedClock(DateTime utc) => Substitute.For<IClock>().Also(c => c.UtcNow.Returns(utc));
    public static IUserContext User(string subject = "Obi-wan Kenobi") => Substitute.For<IUserContext>().Also(u => u.Subject.Returns(subject));
    public static IPiiPolicy Pii() => new PassthroughPiiPolicy();

    public static IValidator<CreateVisitRequest> CreateValidator() => new CreateVisitRequestValidator();

    public static IIdempotencyStore Idem(bool reserved = true)
    {
        var s = Substitute.For<IIdempotencyStore>();
        s.TryReserveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(reserved);
        s.CompleteAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        return s;
    }

    public static VisitService Service(
        IVisitRepository repo,
        IClock? clock = null,
        IUserContext? user = null,
        IIdempotencyStore? idem = null,
        IPiiPolicy? pii = null,
        IDriverRepository? driver = null,
        ILogger<VisitService>? log = null,
        IMemoryCache? cache = null) => new(
            repo,
            CreateValidator(),
            clock ?? FixedClock(DateTime.UtcNow),
            pii ?? Pii(),
            idem ?? Idem(),
            user ?? User(),
            driver ?? Substitute.For<IDriverRepository>(),
            log ?? NullLogger<VisitService>.Instance,
            cache ?? new MemoryCache(new MemoryCacheOptions())
        );

    private static T Also<T>(this T obj, Action<T> act) { act(obj); return obj; }
}