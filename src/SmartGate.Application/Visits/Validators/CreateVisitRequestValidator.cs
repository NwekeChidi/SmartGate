using FluentValidation;
using SmartGate.Domain.Visits;

namespace SmartGate.Application.Visits.Dto;

public sealed class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequest>
{
    public CreateVisitRequestValidator()
    {
        RuleFor(x => x.TruckLicensePlate).NotEmpty().MaximumLength(32);

        RuleFor(x => x.Driver).NotNull();
        RuleFor(x => x.Driver.FirstName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Driver.LastName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Driver.Id)
            .NotEmpty().WithMessage("Driver.Id is required")
            .MinimumLength(8).WithMessage("Driver.Id must be at least 8 characters long")
            .Must(id => id != null && id.StartsWith("DFDS")).WithMessage("Driver.Id must start with 'DFDS'");

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
            .Equal(nameof(VisitStatus.PreRegistered))
            .WithMessage("New visits must have status 'PreRegistered'")
            .When(x => x.Status is not null);
    }
}