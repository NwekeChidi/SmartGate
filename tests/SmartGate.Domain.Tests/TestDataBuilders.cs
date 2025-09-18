using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Domain.Tests;

public static class TestData
{
    public static Truck Truck(string plateRaw = " ab-122 cd ") => new(plateRaw);
    public static Driver Driver(string firstName = "Anakin", string lastName = "Skywalker") => new(firstName, lastName);

    public static Activity Delivery(string unitNumberRaw = "tysi 2/00") => new(ActivityType.Delivery, unitNumberRaw);
    public static Activity Collection(string unitNumberRaw = "tysi 2/00") => new(ActivityType.Collection, unitNumberRaw);

    public static Visit Visit(Truck? truck = null, Driver? driver = null, IEnumerable<Activity>? activities = null, DateTime? nowUTC = null)
        => new(truck ?? Truck(), driver ?? Driver(), activities ?? [ Delivery() ], nowUTC: nowUTC);
}