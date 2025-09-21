using FluentAssertions;
using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Domain.Tests;

public class GuardClauseSpecs
{
    [Fact]
    public void Null_truck_rejected()
    {
        var truckVisit = () => TestData.VisitWithNonNull(
            truck: null!,
            driver: TestData.Driver(),
            activities: [TestData.Delivery()],
            nowUTC: DateTime.UtcNow);
        truckVisit.Should().Throw<NullReferenceInAggregateException>();
    }

    [Fact]
    public void Null_driver_rejected()
    {
        var truckVisit = () => TestData.VisitWithNonNull(
            truck: TestData.Truck(),
            driver: null!,
            activities: [TestData.Delivery()],
            nowUTC: DateTime.UtcNow);
        truckVisit.Should().Throw<NullReferenceInAggregateException>();
    }

    [Fact]
    public void Null_activities_collection_rejected()
    {
        var truckVisit = () => TestData.VisitWithNonNull(
            truck: TestData.Truck(),
            driver: TestData.Driver(),
            activities: null!,
            nowUTC: DateTime.UtcNow);
        truckVisit.Should().Throw<NullReferenceInAggregateException>();
    }

    [Fact]
    public void Null_unit_number_rejected()
    {
        var activity = () => new Activity(ActivityType.Delivery, null!);
        activity.Should().Throw<NullReferenceInAggregateException>();
    }

    [Fact]
    public void Whitespace_unit_number_rejected()
    {
        var activity = () => new Activity(ActivityType.Delivery, "   ");
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
        var driver1 = () => new Driver(longName, "Ok", "dfds-202034");
        var driver2 = () => new Driver("Ok", longName, "dfds-202456");

        driver1.Should().Throw<MaxLengthExceededException>();
        driver2.Should().Throw<MaxLengthExceededException>();
    }

    [Fact]
    public void Invalid_driverId_rejected()
    {
        var driver1 = () => new Driver("Ok", "Ok", "abc-202034");
        var driver2 = () => new Driver("Ok", "Ok", "dfds-2024567890123");
        var driver3 = () => new Driver("Ok", "Ok", "dfds-");
        var driver4 = () => new Driver("Ok", "Ok", "dfds-202/*---");

        driver1.Should().Throw<InvalidDriverIdException>();
        driver2.Should().Throw<MaxLengthExceededException>();
        driver3.Should().Throw<InvalidDriverIdException>();
        driver4.Should().Throw<InvalidDriverIdException>();
    }

    [Fact]
    public void Empty_or_whitespace_driver_names_rejected()
    {
        var driver1 = () => new Driver("    ", "  OK", "dfds-202034");
        var driver2 = () => new Driver("OK", "    ", "dfds-202034");
        driver1.Should().Throw<NullReferenceInAggregateException>();
        driver2.Should().Throw<NullReferenceInAggregateException>();
    }
}