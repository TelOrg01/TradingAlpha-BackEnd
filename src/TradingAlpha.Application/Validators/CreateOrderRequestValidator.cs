using FluentValidation;
using TradingAlpha.Application.Common;
using TradingAlpha.Contracts.Payment;

namespace TradingAlpha.Application.Validators;

/// <summary>
/// Validates the CreateOrderRequest before any business logic runs.
/// Follows the same pattern as RegisterRequestValidator and LoginRequestValidator.
/// </summary>
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty()
            .WithMessage("Plan ID is required")
            .Must(planId => PlanCatalog.Exists(planId))
            .WithMessage("Invalid plan. Available plans: starter, pro, lifetime");
    }
}