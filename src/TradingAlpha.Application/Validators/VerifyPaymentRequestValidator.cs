using FluentValidation;
using TradingAlpha.Contracts.Payment;

namespace TradingAlpha.Application.Validators;

/// <summary>
/// Validates the VerifyPaymentRequest — ensures all three Razorpay tokens are present.
/// These come from the Razorpay checkout handler callback.
/// </summary>
public class VerifyPaymentRequestValidator : AbstractValidator<VerifyPaymentRequest>
{
    public VerifyPaymentRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Razorpay Order ID is required");

        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Razorpay Payment ID is required");

        RuleFor(x => x.Signature)
            .NotEmpty()
            .WithMessage("Razorpay signature is required");
    }
}