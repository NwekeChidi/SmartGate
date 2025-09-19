using NSubstitute;
using SmartGate.Application.Abstractions;
using SmartGate.Application.Visits;
using SmartGate.Application.Visits.Dto;
using SmartGate.Application.Visits.Ports;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartGate.Application.Tests;

public static class TestHelpers
{
    public static IClock FixedClock(DateTime utc) => Substitute.For<IClock>().Also(c => c.UTCNow.Returns(utc));
    public static IUserContext User(string subject = "Obi-wan Kenobi") => Substitute.For<IUserContext>().Also(u => u.Subject.Returns(subject));
    public static IPiiPolicy Pii() => new PassthroughPiiPolicy();

    public static IValidator<CreateVisitRequest> CreateValidator() => new CreateVisitRequestValidator();
    public static IValidator<UpdateVisitStatusRequest> UpdateValidator() => new UpdateVisitStatusRequestValidator();

    public static IIdempotencyStore Idem(bool exists = false, Guid? id = null)
    {
        var s = Substitute.For<IIdempotencyStore>();
        s.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(exists);
        s.GetVisitIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(id);
        return s;
    }

    public static VisitService Service(
        IVisitRepository repo,
        IClock? clock = null,
        IUserContext? user = null,
        IIdempotencyStore? idem = null,
        IPiiPolicy? pii = null,
        ILogger<VisitService>? log = null) => new VisitService(
            repo,
            CreateValidator(),
            UpdateValidator(),
            clock ?? FixedClock(DateTime.UtcNow),
            pii ?? Pii(),
            idem ?? Idem(),
            user ?? User(),
            log ?? NullLogger<VisitService>.Instance
        );

    private static T Also<T>(this T obj, Action<T> act) { act(obj); return obj; }
}