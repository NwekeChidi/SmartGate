namespace SmartGate.Domain.Visits.Entities;

public sealed class Driver
{
    public const int MaxNameLength = 128;
    public const int MaxDriverIdLength = 11;
    public const string DriverIdPrefix = "DFDS-";
    public string Id { get; }
    public string FirstName { get; }
    public string LastName { get; }

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
            throw new InvalidDriverIdException("DriverId must include at least one alphanumeric after DFDS-.");

        if (up.Length > MaxDriverIdLength)
            throw new MaxLengthExceededException(nameof(Id), MaxDriverIdLength);

        var suffix = up.Substring(DriverIdPrefix.Length);
        if (!suffix.All(char.IsLetterOrDigit))
            throw new InvalidDriverIdException("DriverId suffix must be alphanumeric (A–Z, 0–9).");

        return up;
    }
    
    public override string ToString() => $"{FirstName} {LastName}";
}