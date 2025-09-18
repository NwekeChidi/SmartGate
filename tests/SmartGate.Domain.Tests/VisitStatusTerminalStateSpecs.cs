using FluentAssertions;
using SmartGate.Domain.Visits;

namespace SmartGate.Domain.Tests;

public class VisitStatusTerminalStateSpecs
{
    [Fact]
    public void Completed_is_terminal()
    {
        var visit = TestData.VisitWith();
        visit.UpdateStatus(VisitStatus.AtGate);
        visit.UpdateStatus(VisitStatus.OnSite);
        visit.UpdateStatus(VisitStatus.Completed);

        var act = () => visit.UpdateStatus(VisitStatus.AtGate);
        act.Should().Throw<CompletedIsTerminalException>();
    }
}