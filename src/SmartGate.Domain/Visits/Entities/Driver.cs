using System.Diagnostics.CodeAnalysis;
using SmartGate.Domain.Common;
namespace SmartGate.Domain.Visits.Entities;

public sealed class Driver
{
    public const int MaxNameLength = DomainConstants.Driver.DriverNameMaxLength;
    public const int MaxDriverIdLength = DomainConstants.Driver.DriverIdLength;
    public const string DriverIdPrefix = DomainConstants.Driver.DriverIdPrefix;
    public string Id { get; }
    public string FirstName { get; }
    public string LastName { get; }

    [ExcludeFromCodeCoverage]
    private Driver()
    {
        Id = null!;
        FirstName = null!;
        LastName = null!;
    }
    public Driver(string firstName, string lastName, string id)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new NullReferenceInAggregateException(nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new NullReferenceInAggregateException(nameof(lastName));

        if (firstName.Length > MaxNameLength)
            throw new MaxLengthExceededException(nameof(firstName), MaxNameLength);

        if (lastName.Length > MaxNameLength)
            throw new MaxLengthExceededException(nameof(lastName), MaxNameLength);

        Id = NormalizeAndValidate(id);
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    private static string NormalizeAndValidate(string input)
    {
        var up = input.Trim().ToUpperInvariant();

        if (!up.StartsWith(DriverIdPrefix))
            throw new InvalidDriverIdException("DriverId must start with DFDS-.");

        if (up.Length <= DriverIdPrefix.Length)
            throw new InvalidDriverIdException("DriverId must include 11 numeric characters after DFDS-.");

        if (up.Length > MaxDriverIdLength)
            throw new MaxLengthExceededException(nameof(Id), MaxDriverIdLength);

        var suffix = up.Substring(DriverIdPrefix.Length);
        if (!suffix.All(char.IsDigit))
            throw new InvalidDriverIdException("DriverId suffix must be numeric (0–9).");

        return up;
    }
}