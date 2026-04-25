using FluentValidation;
using CrmWorkTrack.Application.Interfaces.Auth.DTOs;

namespace CrmWorkTrack.Application.Interfaces.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(200);
    }
}