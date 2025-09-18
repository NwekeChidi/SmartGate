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

        RuleFor(x => x.Activities).NotNull().Must(a => a.Count > 0).WithMessage("At least one activity is required");

        RuleForEach(x => x.Activities).ChildRules(activity =>
        {
            activity.RuleFor(a => a.UnitNumberRaw)
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