using FluentValidation;
using CrmWorkTrack.Application.Features.CustomerContacts.Dtos;

namespace CrmWorkTrack.Application.Features.CustomerContacts.Validators;

public class UpdateCustomerContactRequestValidator : AbstractValidator<UpdateCustomerContactRequest>
{
    public UpdateCustomerContactRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Title)
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .MaximumLength(200)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(50);

        RuleFor(x => x.MobilePhone)
            .MaximumLength(50);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
