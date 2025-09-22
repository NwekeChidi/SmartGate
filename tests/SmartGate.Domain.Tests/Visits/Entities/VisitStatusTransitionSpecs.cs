using FluentAssertions;
using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Domain.Tests.Visits.Entities;

public class VisitStatusTransitionSpecs
{
    private static Visit NewVisit() => TestData.VisitWith();

    [Fact]
    public void PreRegistered_to_AtGate_is_allowed()
    {
        var visit = NewVisit();
        visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM");
        visit.Status.Should().Be(VisitStatus.AtGate);
    }

    [Fact]
    public void AtGate_to_OnSite_is_allowed()
    {
        var visit = NewVisit();
        visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM");
        visit.UpdateStatus(VisitStatus.OnSite, "SYSTEM");
        visit.Status.Should().Be(VisitStatus.OnSite);
    }

    [Fact]
    public void OnSite_to_Completed_is_allowed()
    {
        var visit = NewVisit();
        visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM");
        visit.UpdateStatus(VisitStatus.OnSite, "SYSTEM");
        visit.UpdateStatus(VisitStatus.Completed, "SYSTEM");
        visit.Status.Should().Be(VisitStatus.Completed, "SYSTEM");
    }

    [Theory]
    [InlineData(VisitStatus.PreRegistered, VisitStatus.OnSite)]
    [InlineData(VisitStatus.PreRegistered, VisitStatus.Completed)]
    [InlineData(VisitStatus.AtGate, VisitStatus.Completed)]
    public void Skipping_ahead_is_rejected(VisitStatus from, VisitStatus to)
    {
        var visit = NewVisit();

        if (from == VisitStatus.AtGate) visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM");

        var activity = () => visit.UpdateStatus(to, "SYSTEM");
        activity.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void Repeating_a_status_is_rejected()
    {
        var visit = NewVisit();
        visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM");
        visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM");
        visit.Status.Should().Be(VisitStatus.AtGate);
    }

    [Fact]
    public void Going_backwards_is_rejected()
    {
        var visit = NewVisit();
        visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM");
        var act = () => visit.UpdateStatus(VisitStatus.PreRegistered, "SYSTEM");
        act.Should().Throw<InvalidStatusTransitionException>();
    }

}