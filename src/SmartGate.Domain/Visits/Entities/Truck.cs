using System.Diagnostics.CodeAnalysis;
using SmartGate.Domain.Common;

namespace SmartGate.Domain.Visits.Entities;

public sealed class Truck
{
    public const int PlateLength = DomainConstants.Truck.TruckPlateLength;
    private const string licensePlateName = DomainConstants.Truck.TruckLicensePlateName;
    public string LicensePlateRaw { get; }
    public string LicensePlateNormalized { get; }

    [ExcludeFromCodeCoverage]
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

        if (normalized.Length != PlateLength)
            throw new InvalidIdentifierLengthException(licensePlateName, PlateLength);

        this.LicensePlateNormalized = normalized;
    }
}