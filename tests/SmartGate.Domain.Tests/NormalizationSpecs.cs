using FluentAssertions;
using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Domain.Tests;

public class NormalizationSpecs
{
    [Fact]
    public void Normalizes_license_plate_uppercase_and_strip_non_alnum()
    {
        var truck = TestData.Truck(" ab-12 cd ");
        truck.LicensePlateRaw.Should().Be(" ab-12 cd ");
        truck.LicensePlateNormalized.Should().Be("AB12CD");
    }

    [Fact]
    public void Normalizes_unit_number_and_enforces_dfds_prefix()
    {
        var activity = TestData.Delivery(" dfds009 ");
        activity.UnitNumberRaw.Should().Be(" dfds009 ");
        activity.UnitNumberNormalized.Should().Be("DFDS009");
    }

    [Fact]
    public void Unit_number_without_dfds_prefix_is_rejected()
    {
        var act = () => new Activity(ActivityType.Delivery, " ZN/009 ");
        act.Should().Throw<UnitNumberMustStartWithDFDSException>();
    }

    [Fact]
    public void Unit_number_with_only_dfds_and_nothing_else_is_rejected()
    {
        var act = () => new Activity(ActivityType.Collection, " dfds ");
        act.Should().Throw<UnitNumberMustStartWithDFDSException>();
    }

    [Fact]
    public void All_non_alphanumeric_unit_after_normalization_is_invalid()
    {
        var act = () => new Activity(ActivityType.Delivery, "---///   ");
        act.Should().Throw<InvalidIdentifierException>();
    }

    [Fact]
    public void Oversized_license_plate_rejected()
    {
        var tooLong = new string('X', Truck.MaxPlateLength + 1);
        var act = () => TestData.Truck(tooLong);
        act.Should().Throw<MaxLengthExceededException>();
    }

    [Fact]
    public void Empty_normalized_license_plate_rejected()
    {
        var act = () => TestData.Truck("  ---/// ");
        act.Should().Throw<InvalidIdentifierException>();
    }

    [Fact]
    public void Oversized_unit_number_rejected()
    {
        var tooLong = new string('Y', Activity.MaxUnitLength + 1);
        var act = () => new Activity(ActivityType.Collection, tooLong);
        act.Should().Throw<MaxLengthExceededException>();
    }
}