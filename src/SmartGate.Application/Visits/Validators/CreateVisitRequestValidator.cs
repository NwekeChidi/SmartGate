using System.Text.RegularExpressions;
using FluentValidation;
using SmartGate.Application.Visits.Dto;
using SmartGate.Domain.Visits;

namespace SmartGate.Application.Visits.Validators;

public sealed partial class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequest>
{
    private static readonly Regex DriverIdRegex = _driverIdRegex();
    private static readonly Regex UnitNumberRegex = _unitNumberRegex();
    private const string cultureName = "en-GB";

    public CreateVisitRequestValidator()
    {
        RuleFor(x => x.TruckLicensePlate)
            .NotEmpty()
            .Length(7);

        RuleFor(x => x.Driver).NotNull();
        RuleFor(x => x.Driver.FirstName)
            .NotEmpty()
            .MaximumLength(128);
        RuleFor(x => x.Driver.LastName)
            .NotEmpty()
            .MaximumLength(128);
        RuleFor(x => x.Driver.Id)
            .NotEmpty()
            .Must(DriverIdRegex.IsMatch)
            .WithMessage("driver.id must match pattern DFDS-<11 numeric characters>.")
            .Length(16);

        RuleFor(x => x.Activities).NotNull().Must(activity => activity.Count > 0).WithMessage("At least one activity is required");
        RuleForEach(x => x.Activities).ChildRules(activity =>
        {
            activity.RuleFor(a => a.UnitNumber)
                .NotEmpty()
                .Must(UnitNumberRegex.IsMatch)
                .WithMessage("activity.unitNumber must match pattern DFDS<6 numeric characters>.")
                .Length(10);
        });

        RuleFor(x => x.IdempotencyKey)
            .NotEqual(Guid.Empty)
            .When(x => x.IdempotencyKey is not null)
            .WithMessage("idempotencyKey must be a valid Guid.");

        RuleFor(x => x.Status)
            .Equal(VisitStatus.PreRegistered)
            .WithMessage("New visits must have status 'PreRegistered'");
    }

    [GeneratedRegex(@"^(?i)dfds-[0-9]{11}$", RegexOptions.Compiled, cultureName)]
    private static partial Regex _driverIdRegex();
    [GeneratedRegex(@"^(?i)dfds[0-9]{6}$", RegexOptions.Compiled, cultureName)]
    private static partial Regex _unitNumberRegex();
}