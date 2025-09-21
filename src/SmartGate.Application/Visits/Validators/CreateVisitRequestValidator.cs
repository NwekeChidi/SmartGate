using System.Text.RegularExpressions;
using FluentValidation;
using SmartGate.Application.Visits.Dto;
using SmartGate.Domain.Visits;

namespace SmartGate.Application.Visits.Validators;

public sealed partial class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequest>
{
    private static readonly Regex DriverIdRegex = _driverIdRegex();

    public CreateVisitRequestValidator()
    {
        RuleFor(x => x.TruckLicensePlate)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(32);

        RuleFor(x => x.Driver).NotNull();
        RuleFor(x => x.Driver.FirstName)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(128);
        RuleFor(x => x.Driver.LastName)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(128);
        RuleFor(x => x.Driver.Id)
            .NotEmpty()
            .Must(DriverIdRegex.IsMatch)
            .WithMessage("Driver.Id must match pattern DFDS-<1..11 numeric characters>.");

        RuleFor(x => x.Activities).NotNull().Must(activity => activity.Count > 0).WithMessage("At least one activity is required");
        RuleForEach(x => x.Activities).ChildRules(activity =>
        {
            activity.RuleFor(a => a.UnitNumber)
                .NotEmpty().MaximumLength(32);
        });

        RuleFor(x => x.IdempotencyKey)
            .NotEqual(Guid.Empty)
            .When(x => x.IdempotencyKey is not null)
            .WithMessage("IdempotencyKey must be a valid .");

        RuleFor(x => x.Status)
            .Equal(VisitStatus.PreRegistered)
            .WithMessage("New visits must have status 'PreRegistered'");
    }

    [GeneratedRegex(@"^(?i)dfds-[a-z0-9]{1,11}$", RegexOptions.Compiled, "en-US")]
    private static partial Regex _driverIdRegex();
}