using SmartGate.Domain.Visits;

namespace SmartGate.Domain.Visits.Entities;

public sealed class Driver
{
    public const int MaxNameLength = 128;

    public string FirstName { get; }
    public string LastName { get; }

    public Driver(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new NullReferenceInAggregateException(nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new NullReferenceInAggregateException(nameof(lastName));

        if (firstName.Length > MaxNameLength)
            throw new MaxLengthExceededException(nameof(firstName), MaxNameLength);

        if (lastName.Length > MaxNameLength)
            throw new MaxLengthExceededException(nameof(lastName), MaxNameLength);

        this.FirstName = firstName.Trim();
        this.LastName = lastName.Trim();
    }
    
    public override string ToString() => $"{this.FirstName} {this.LastName}";
}