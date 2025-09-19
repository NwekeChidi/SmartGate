using SmartGate.Domain.Common;

namespace SmartGate.Domain.Visits.Entities;

public sealed class Truck
{
    public const int MaxPlateLength = 7;
    private const string licensePlateName = "truckLicensePlate";
    public string LicensePlateRaw { get; }
    public string LicensePlateNormalized { get; }

    public Truck()
    {
        LicensePlateRaw = null!;
        LicensePlateNormalized = null!;
    }

    public Truck(string licensePlateRaw)
    {
        if (string.IsNullOrWhiteSpace(licensePlateRaw))
            throw new NullReferenceInAggregateException(licensePlateName);

        this.LicensePlateRaw = licensePlateRaw;
        var normalized = Normalization.NormalizePlateOrUnit(licensePlateRaw);

        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidIdentifierException(licensePlateName);

        if (normalized.Length > MaxPlateLength)
            throw new MaxLengthExceededException(licensePlateName, MaxPlateLength);

        this.LicensePlateNormalized = normalized;
    }
}