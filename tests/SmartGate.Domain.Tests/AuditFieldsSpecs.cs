using FluentAssertions;

namespace SmartGate.Domain.Tests;

public class AuditFieldsSpecs
{
    [Fact]
    public void Created_and_Updated_set_on_create_and_updated_on_transition()
    {
        var t0 = new DateTime(2025, 9, 17, 8, 30, 0, DateTimeKind.Utc);
        var visit = TestData.VisitWith(nowUTC: t0);
        visit.CreatedAtUTC.Should().Be(t0);
        visit.UpdatedAtUTC.Should().Be(t0);

        var t1 = t0.AddMinutes(5);
        visit.UpdateStatus(Visits.VisitStatus.AtGate, "SYSTEM", t1);
        visit.UpdatedAtUTC.Should().Be(t1);
        visit.CreatedAtUTC.Should().Be(t0);
    }
}