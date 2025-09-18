using FluentAssertions;
using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Domain.Tests;

public class GuardClauseSpecs
{
    [Fact]
    public void Null_truck_rejected()
    {
        var activity = () => TestData.VisitWith(truck: null!);
        activity.Should().Throw<NullReferenceInAggregateException>();
    }

    [Fact]
    public void Null_driver_rejected()
    {
        var activity = () => TestData.VisitWith(driver: null!);
        activity.Should().Throw<NullReferenceInAggregateException>();
    }

    [Fact]
    public void Null_activities_collection_rejected()
    {
        var activity = () => TestData.VisitWith(activities: null!);
        activity.Should().Throw<NullReferenceInAggregateException>();
    }

    [Fact]
    public void Null_unit_number_rejected()
    {
        var activity = () => new Activity(ActivityType.Delivery, null!);
        activity.Should().Throw<NullReferenceInAggregateException>();
    }

    [Fact]
    public void Null_plate_rejected()
    {
        var activity = () => new Truck(null!);
        activity.Should().Throw<NullReferenceInAggregateException>();
    }

    [Fact]
    public void Oversized_driver_names_rejected()
    {
        var longName = new string('a', Driver.MaxNameLength + 1);
        var activity1 = () => new Driver(longName, "Ok");
        var activity2 = () => new Driver("Ok", longName);

        activity1.Should().Throw<MaxLengthExceededException>();
        activity2.Should().Throw<MaxLengthExceededException>();
    }
}