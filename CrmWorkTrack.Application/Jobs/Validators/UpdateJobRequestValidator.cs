using FluentValidation;
using CrmWorkTrack.Application.Jobs.DTOs;

namespace CrmWorkTrack.Application.Jobs.Validators;

public class UpdateJobRequestValidator : AbstractValidator<UpdateJobRequest>
{
    public UpdateJobRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Title))
            .WithMessage("Title must be at most 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null)
            .WithMessage("Description must be at most 2000 characters.");

        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be one of: open, inprogress, completed, cancelled.");

        RuleFor(x => x.Priority)
            .Must(BeValidPriority)
            .When(x => !string.IsNullOrWhiteSpace(x.Priority))
            .WithMessage("Priority must be one of: Low, Medium, High.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.DueDate.HasValue)
            .WithMessage("DueDate must be in the future.");
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        var s = status.Trim().ToLowerInvariant();
        return s is "open" or "inprogress" or "completed" or "cancelled";
    }

    private static bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return false;

        var p = priority.Trim().ToLowerInvariant();
        return p is "low" or "medium" or "high";
    }
}
