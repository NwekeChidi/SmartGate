using FluentAssertions;
using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Domain.Tests;

public class VisitStatusTransitionSpecs
{
    private static Visit NewVisit() => TestData.VisitWith();

    [Fact]
    public void PreRegistered_to_AtGate_is_allowed()
    {
        var visit = NewVisit();
        visit.UpdateStatus(VisitStatus.AtGate);
        visit.Status.Should().Be(VisitStatus.AtGate);
    }

    [Fact]
    public void AtGate_to_OnSite_is_allowed()
    {
        var visit = NewVisit();
        visit.UpdateStatus(VisitStatus.AtGate);
        visit.UpdateStatus(VisitStatus.OnSite);
        visit.Status.Should().Be(VisitStatus.OnSite);
    }

    [Fact]
    public void OnSite_to_Completed_is_allowed()
    {
        var visit = NewVisit();
        visit.UpdateStatus(VisitStatus.AtGate);
        visit.UpdateStatus(VisitStatus.OnSite);
        visit.UpdateStatus(VisitStatus.Completed);
        visit.Status.Should().Be(VisitStatus.Completed);
    }

    [Theory]
    [InlineData(VisitStatus.PreRegistered, VisitStatus.OnSite)]
    [InlineData(VisitStatus.PreRegistered, VisitStatus.Completed)]
    [InlineData(VisitStatus.AtGate, VisitStatus.Completed)]
    public void Skipping_ahead_is_rejected(VisitStatus from, VisitStatus to)
    {
        var visit = NewVisit();

        if (from == VisitStatus.AtGate) visit.UpdateStatus(VisitStatus.AtGate);

        var activity = () => visit.UpdateStatus(to);
        activity.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void Repeating_a_status_is_rejected()
    {
        var visit = NewVisit();
        visit.UpdateStatus(VisitStatus.AtGate);
        visit.UpdateStatus(VisitStatus.AtGate);
        visit.Status.Should().Be(VisitStatus.AtGate);
    }

    [Fact]
    public void Going_backwards_is_rejected()
    {
        var visit = NewVisit();
        visit.UpdateStatus(VisitStatus.AtGate);
        var act = () => visit.UpdateStatus(VisitStatus.PreRegistered);
        act.Should().Throw<InvalidStatusTransitionException>();
    }

}