using FluentValidation;
using CrmWorkTrack.Application.Customers.DTOs;

namespace CrmWorkTrack.Application.Customers.Validators;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200);

        RuleFor(x => x.ContactName)
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("A valid email address is required.")
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .MaximumLength(30);

        RuleFor(x => x.Address)
            .MaximumLength(500);

        RuleFor(x => x.City)
            .MaximumLength(100);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
