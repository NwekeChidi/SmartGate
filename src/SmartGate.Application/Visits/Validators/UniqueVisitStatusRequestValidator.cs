using FluentValidation;
using SmartGate.Domain.Visits;

namespace SmartGate.Application.Visits.Dto;

public sealed class UpdateVisitStatusRequestValidator : AbstractValidator<UpdateVisitStatusRequest>
{
    public UpdateVisitStatusRequestValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .Must(s => s is VisitStatus.PreRegistered or VisitStatus.AtGate or VisitStatus.OnSite or VisitStatus.Completed);
    }
}