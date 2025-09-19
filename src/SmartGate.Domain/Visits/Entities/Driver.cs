namespace SmartGate.Domain.Visits.Entities;

public sealed class Driver
{
    public const int MaxNameLength = 128;
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

        Id = id;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }
    
    public override string ToString() => $"{FirstName} {LastName}";
}