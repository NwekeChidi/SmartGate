using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Domain.Tests;

public static class TestData
{
    public static Truck Truck(string licencePlateRaw = " ab-12 cd ") => new(licencePlateRaw);
    public static Driver Driver(string firstName = "Anakin", string lastName = "Skywalker") => new(firstName, lastName);

    public static Activity Delivery(string unitNumberRaw = " dfds-789576 ")
        => new(ActivityType.Delivery, unitNumberRaw);

    public static Activity Collection(string unitNumberRaw = " dfds189576 ")
        => new(ActivityType.Collection, unitNumberRaw);

    public static Visit VisitWith(
        Truck? truck = null,
        Driver? driver = null,
        IEnumerable<Activity>? activities = null,
        DateTime? nowUTC = null)
        => new(
            truck ?? Truck(),
            driver ?? Driver(),
            activities ?? [Delivery("DFDS11001")],
            id: null,
            nowUTC: nowUTC);
    public static Visit VisitWithNonNull(
        Truck truck,
        Driver driver,
        IEnumerable<Activity> activities,
        DateTime nowUTC)
        => new(
            truck,
            driver,
            activities,
            id: null,
            nowUTC);
}