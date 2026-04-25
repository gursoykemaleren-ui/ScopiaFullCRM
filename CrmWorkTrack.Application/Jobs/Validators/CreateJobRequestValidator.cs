using FluentValidation;
using CrmWorkTrack.Application.Jobs.DTOs;

namespace CrmWorkTrack.Application.Jobs.Validators;

public class CreateJobRequestValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId must be greater than 0.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(200)
            .WithMessage("Title must be at most 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null)
            .WithMessage("Description must be at most 2000 characters.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required.")
            .Must(BeValidStatus)
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
