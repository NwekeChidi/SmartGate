using FluentAssertions;
using SmartGate.Domain.Visits;

namespace SmartGate.Domain.Tests.Visits.Entities;

public class VisitCreationSpecs
{
    [Fact]
    public void Creates_visit_with_delivery_only_and_sets_defaults()
    {
        var now = new DateTime(2025, 9, 18, 12, 0, 0, DateTimeKind.Utc);
        var truck = TestData.Truck(" ab-12 cd ");
        var driver = TestData.Driver("Harry", "Potter");
        var activities = new[] { TestData.Delivery(" dfds-789009 ") };
        var visit = TestData.VisitWith(truck, driver, activities, now);

        visit.Status.Should().Be(VisitStatus.PreRegistered);
        visit.CreatedAtUTC.Should().Be(now);
        visit.UpdatedAtUTC.Should().Be(now);

        visit.Truck.LicensePlateRaw.Should().Be(" ab-12 cd ");
        visit.Truck.LicensePlateNormalized.Should().Be("AB12CD");

        var activity = visit.Activities.Single();
        activity.UnitNumberRaw.Should().Be(" dfds-789009 ");
        activity.UnitNumberNormalized.Should().Be("DFDS789009");
    }

    [Fact]
    public void Creates_visit_with_collection_only()
    {
        var visit = TestData.VisitWith(activities: [TestData.Collection(" dfDS214777 ")]);

        visit.Status.Should().Be(VisitStatus.PreRegistered);
        visit.Activities.Should().HaveCount(1);
        visit.Activities[0].Type.Should().Be(ActivityType.Collection);
        visit.Activities[0].UnitNumberNormalized.Should().Be("DFDS214777");
    }

    [Fact]
    public void Creates_visit_with_delivery_and_collection()
    {
        var visit = TestData.VisitWith(activities: [
            TestData.Delivery(" DFDS11001 ", id: Guid.NewGuid()),
            TestData.Collection(" DFDS22002 ")
        ]);

        visit.Status.Should().Be(VisitStatus.PreRegistered);
        visit.Activities.Should().HaveCount(2);
        visit.Activities[0].Type.Should().Be(ActivityType.Delivery);
        visit.Activities[0].UnitNumberNormalized.Should().Be("DFDS11001");
        visit.Activities[1].Type.Should().Be(ActivityType.Collection);
        visit.Activities[1].UnitNumberNormalized.Should().Be("DFDS22002");
    }

    [Fact]
    public void Creating_visit_with_no_activities_fails()
    {
        var act = () => TestData.VisitWith(activities: []);
        act.Should().Throw<ActivitiesRequiredException>();
    }
}