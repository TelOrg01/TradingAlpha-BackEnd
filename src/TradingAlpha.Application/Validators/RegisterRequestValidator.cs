using FluentValidation;
using TradingAlpha.Contracts.Auth;

namespace TradingAlpha.Application.Validators;

/// <summary>
/// Validates the RegisterRequest DTO before it reaches AuthService.
/// 
/// Catches bad input early — before any database call or hashing.
/// FluentValidation integrates with ASP.NET's model validation pipeline
/// so errors are returned as 400 Bad Request automatically.
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}