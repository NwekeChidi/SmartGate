using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Domain.Tests;

public static class TestData
{
    public static Truck Truck(string licencePlateRaw = " ab-12 cd ") => new(licencePlateRaw);
    public static Driver Driver(string firstName = "Anakin", string lastName = "Skywalker") => new(firstName, lastName);

    public static Activity Delivery(string unitNumberRaw = " zn/009 ")
        => new(ActivityType.Delivery, unitNumberRaw);

    public static Activity Collection(string unitNumberRaw = " bx-777 ")
        => new(ActivityType.Collection, unitNumberRaw);

    public static Visit VisitWith(
        Truck? truck = null,
        Driver? driver = null,
        IEnumerable<Activity>? activities = null,
        DateTime? nowUTC = null)
        => new(
            truck ?? Truck(),
            driver ?? Driver(),
            activities ?? [Delivery("UN-001")],
            id: null,
            nowUTC: nowUTC);
}