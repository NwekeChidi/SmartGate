namespace SmartGate.Domain.Common;

public static partial class DomainConstants
{
    public static class Driver
    {
        public const string DriverIdPrefix = "DFDS-";
        public const int DriverIdLength = 16;
        public const int DriverNameMaxLength = 128;
    }

    public static class Truck
    {
        public const int TruckPlateLength = 7;
        public const string TruckLicensePlateName = "truckLicensePlate";

    }

    public static class Activity
    {
        public const int UnitNumberLength = 10;
        public const string RequiredUnitPrefix = "DFDS";
        public const string UnitNumberName = "unitNumber";
    }
}