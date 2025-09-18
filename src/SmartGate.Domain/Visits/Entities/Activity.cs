using SmartGate.Domain.Common;

namespace SmartGate.Domain.Visits.Entities;

public sealed class Activity
{
    public const int MaxUnitLength = 10;
    public const string RequiredUnitPrefix = "DFDS";

    public Guid Id { get; }
    public ActivityType Type { get; }
    public string UnitNumberRaw { get; }
    public string UnitNumberNormalized { get; }

    public Activity(ActivityType type, string unitNumberRaw, Guid? id = null)
    {
        if (string.IsNullOrEmpty(unitNumberRaw))
            throw new NullReferenceInAggregateException(nameof(unitNumberRaw));

        if (unitNumberRaw.Length > MaxUnitLength)
            throw new MaxLengthExceededException(nameof(unitNumberRaw), MaxUnitLength);

        this.Id = id ?? Guid.NewGuid();
        this.Type = type;

        this.UnitNumberRaw = unitNumberRaw;
        var normalized = Normalization.NormalizePlateOrUnit(unitNumberRaw);
        
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidIdentifierException(nameof(this.UnitNumberRaw));

        if (!normalized.StartsWith(RequiredUnitPrefix, StringComparison.Ordinal) ||
            normalized.Length <= RequiredUnitPrefix.Length)
            throw new UnitNumberMustStartWithDFDSException();

        this.UnitNumberNormalized = normalized;
    }

}