using FluentValidation;
using CrmWorkTrack.Application.Interfaces.Auth.DTOs;

namespace CrmWorkTrack.Application.Interfaces.Auth.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.")
            .MaximumLength(1000);
    }
}
