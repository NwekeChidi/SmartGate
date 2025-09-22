using FluentAssertions;
using SmartGate.Domain.Visits;

namespace SmartGate.Domain.Tests.Visits.Entities;

public class VisitStatusTerminalStateSpecs
{
    [Fact]
    public void Completed_is_terminal()
    {
        var visit = TestData.VisitWith();
        visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM");
        visit.UpdateStatus(VisitStatus.OnSite, "SYSTEM");
        visit.UpdateStatus(VisitStatus.Completed, "SYSTEM");

        var act = () => visit.UpdateStatus(VisitStatus.AtGate, "SYSTEM");
        act.Should().Throw<CompletedIsTerminalException>();
    }
}