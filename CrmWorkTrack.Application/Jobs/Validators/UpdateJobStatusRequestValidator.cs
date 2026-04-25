using FluentValidation;
using CrmWorkTrack.Application.Jobs.DTOs;

namespace CrmWorkTrack.Application.Jobs.Validators;

public sealed class UpdateJobStatusRequestValidator : AbstractValidator<UpdateJobStatusRequest>
{
    public UpdateJobStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(BeValidStatus)
            .WithMessage("Status must be one of: open, inprogress, completed, cancelled.");
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        var s = status.Trim().ToLowerInvariant();

        return s is "open" or "inprogress" or "completed" or "cancelled";
    }
}