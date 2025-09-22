using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Domain.Tests;

public static class TestData
{
    public static Truck Truck(string licencePlateRaw = " ab-123cd ") => new(licencePlateRaw);
    public static Driver Driver(
        string firstName = "Anakin",
        string lastName = "Skywalker",
        string id = "dfds-202435467") => new(firstName, lastName, id);

    public static Activity Delivery(string unitNumberRaw = " dfds-789576 ", Guid? id = null)
        => new(ActivityType.Delivery, unitNumberRaw, id);

    public static Activity Collection(string unitNumberRaw = " dfds189576 ", Guid? id = null)
        => new(ActivityType.Collection, unitNumberRaw, id);

    public static Visit VisitWith(
        Truck? truck = null,
        Driver? driver = null,
        IEnumerable<Activity>? activities = null,
        DateTime? nowUTC = null)
        => new(
            truck ?? Truck(),
            driver ?? Driver(),
            activities ?? [Delivery("DFDS-110012")],
            createdBy: "TEST",
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
            createdBy: "TEST",
            id: null,
            nowUTC);
}