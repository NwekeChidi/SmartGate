using System.Text.RegularExpressions;
using FluentValidation;
using SmartGate.Domain.Visits;

namespace SmartGate.Application.Visits.Dto;

public sealed partial class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequest>
{
    private static readonly Regex DriverIdRegex = _driverIdRegex();

    public CreateVisitRequestValidator()
    {
        RuleFor(x => x.TruckLicensePlate).NotEmpty().MaximumLength(32);

        RuleFor(x => x.Driver).NotNull();
        RuleFor(x => x.Driver.FirstName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Driver.LastName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Driver.Id)
            .NotEmpty()
            .Must(DriverIdRegex.IsMatch)
            .WithMessage("Driver.Id must match pattern DFDS-<1..6 alphanumeric characters>.");

        RuleFor(x => x.Activities).NotNull().Must(a => a.Count > 0).WithMessage("At least one activity is required");

        RuleForEach(x => x.Activities).ChildRules(activity =>
        {
            activity.RuleFor(a => a.UnitNumber)
                .NotEmpty().MaximumLength(32);
        });

        RuleFor(x => x.IdempotencyKey)
            .MaximumLength(128)
            .When(x => x.IdempotencyKey is not null);
        
        RuleFor(x => x.Status)
            .Equal(VisitStatus.PreRegistered)
            .WithMessage("New visits must have status 'PreRegistered'")
            .When(x => x.Status is not null);
    }

    [GeneratedRegex(@"^(?i)dfds-[a-z0-9]{1,6}$", RegexOptions.Compiled, "en-US")]
    private static partial Regex _driverIdRegex();
}