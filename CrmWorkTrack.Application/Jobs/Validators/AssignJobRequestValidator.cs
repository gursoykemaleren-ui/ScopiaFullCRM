using FluentValidation;
using CrmWorkTrack.Application.Jobs.DTOs;

namespace CrmWorkTrack.Application.Jobs.Validators;

public sealed class AssignJobRequestValidator : AbstractValidator<AssignJobRequest>
{
    public AssignJobRequestValidator()
    {
        RuleFor(x => x.AssignedToUserId)
            .GreaterThan(0).WithMessage("AssignedToUserId must be greater than 0.");
    }
}
