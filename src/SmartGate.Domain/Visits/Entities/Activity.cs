using System.Diagnostics.CodeAnalysis;
using SmartGate.Domain.Common;

namespace SmartGate.Domain.Visits.Entities;

public sealed class Activity
{
    public const int MaxUnitLength = DomainConstants.Activity.MaxUnitLength;
    private const string UnitNumberName = DomainConstants.Activity.UnitNumberName;
    public const string RequiredUnitPrefix = DomainConstants.Activity.RequiredUnitPrefix;

    public Guid Id { get; }
    public ActivityType Type { get; }
    public string UnitNumberRaw { get; }
    public string UnitNumberNormalized { get; }

    [ExcludeFromCodeCoverage]
    private Activity()
    {
        UnitNumberNormalized = null!;
        UnitNumberRaw = null!;
    }

    public Activity(ActivityType type, string unitNumberRaw, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(unitNumberRaw))
            throw new NullReferenceInAggregateException(UnitNumberName);

        Id = id ?? Guid.NewGuid();
        Type = type;

        UnitNumberRaw = unitNumberRaw;
        var normalized = Normalization.NormalizePlateOrUnit(unitNumberRaw);

        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidIdentifierException(UnitNumberName);

        if (normalized.Length > MaxUnitLength)
            throw new MaxLengthExceededException(UnitNumberName, MaxUnitLength);

        if (!normalized.StartsWith(RequiredUnitPrefix, StringComparison.Ordinal) ||
            normalized.Length <= RequiredUnitPrefix.Length)
            throw new UnitNumberMustStartWithDFDSException();

        UnitNumberNormalized = normalized;
    }

}