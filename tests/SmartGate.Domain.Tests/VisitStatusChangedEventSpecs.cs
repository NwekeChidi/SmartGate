using FluentAssertions;
using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Events;

namespace SmartGate.Domain.Tests;

public class VisitStatusChangedEventSpecs
{
    [Fact]
    public void Raises_event_on_valid_transition_with_correct_payload()
    {
        var now = new DateTime(2025, 9, 18, 12, 0, 0, DateTimeKind.Utc);
        var visit = TestData.VisitWith(nowUTC: now);
        visit.DomainEvents.Should().BeEmpty();

        var after = now.AddMinutes(1);
        visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM", after);

        visit.DomainEvents.Should().HaveCount(1);
        var evt = visit.DomainEvents.Single().Should().BeOfType<VisitStatusChanged>().Subject;
        evt.VisitId.Should().Be(visit.Id);
        evt.OldStatus.Should().Be(VisitStatus.PreRegistered);
        evt.NewStatus.Should().Be(VisitStatus.AtGate);
        evt.OccurredAtUtc.Should().Be(after);
    }

    [Fact]
    public void No_event_when_advancing_to_same_status_idempotent()
    {
        var visit = TestData.VisitWith();
        visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM");
        visit.ClearDomainEvents();

        visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM");
        visit.DomainEvents.Should().BeEmpty();
    }
}