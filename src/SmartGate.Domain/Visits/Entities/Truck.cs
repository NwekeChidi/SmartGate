using SmartGate.Domain.Common;

namespace SmartGate.Domain.Visits.Entities;

public sealed class Truck
{
    public const int MaxPlateLength = 7;
    public string LicensePlateRaw { get; }
    public string LicensePlateNormalized { get; }

    public Truck(string licensePlateRaw)
    {
        if (string.IsNullOrWhiteSpace(licensePlateRaw)) throw new NullReferenceInAggregateException(nameof(licensePlateRaw));
        if (licensePlateRaw.Length > MaxPlateLength) throw new MaxLengthExceededException(nameof(licensePlateRaw), MaxPlateLength);

        this.LicensePlateRaw = licensePlateRaw;
        var normalized = Normalization.NormalizePlateOrUnit(licensePlateRaw);
        if (string.IsNullOrWhiteSpace(normalized)) throw new InvalidIdentifierException(nameof(licensePlateRaw));

        this.LicensePlateNormalized = normalized;
    }
}