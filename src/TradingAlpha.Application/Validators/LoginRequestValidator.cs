using FluentValidation;
using TradingAlpha.Contracts.Auth;

namespace TradingAlpha.Application.Validators;

/// <summary>
/// Validates the LoginRequest DTO — ensures both fields are present.
/// Actual credential verification happens in AuthService.
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}