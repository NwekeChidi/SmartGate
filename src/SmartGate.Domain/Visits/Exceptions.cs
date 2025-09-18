using SmartGate.Domain.Common;

namespace SmartGate.Domain.Visits;

public sealed class ActivitiesRequiredException : DomainException
{
    public ActivitiesRequiredException() : base("At least one activity is required.") { }
}

public sealed class InvalidIdentifierException(string field) : DomainException($"Please provide a valid {field}.")
{
}

public sealed class MaxLengthExceededException(string field, int max) : DomainException($"{field} exceeds the allowed maximum length of {max}.")
{
}

public sealed class NullReferenceInAggregateException(string field) : DomainException($"{field} cannot be null.")
{
}

public sealed class InvalidStatusTransitionException(VisitStatus from, VisitStatus to)
    : DomainException($"Transition from {from} to {to} is not allowed.")
{
}

public sealed class CompletedIsTerminalException() : DomainException("Visit is already Completed and cannot be changed.")
{
}

public sealed class UnitNumberMustStartWithDFDSException() : DomainException("Unit number must start with 'DFDS'.")
{
}
