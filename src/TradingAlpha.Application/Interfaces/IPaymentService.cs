using TradingAlpha.Contracts.Payment;
using TradingAlpha.Domain.Common;

namespace TradingAlpha.Application.Interfaces;

/// <summary>
/// Payment orchestration service. Coordinates between Razorpay gateway,
/// repositories, and the subscription lifecycle.
/// Returns Result&lt;T&gt; to match the existing AuthService pattern.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a Razorpay order for the given plan.
    /// Validates planId, resolves price server-side, calls Razorpay API,
    /// and stores a Pending payment record.
    /// </summary>
    Task<Result<CreateOrderResponse>> CreateOrderAsync(string planId, Guid userId);

    /// <summary>
    /// Verifies a completed Razorpay payment and activates the subscription.
    /// Validates signature, updates payment record to Success,
    /// creates a new Subscription with Active status.
    /// </summary>
    Task<Result<VerifyPaymentResponse>> VerifyPaymentAsync(VerifyPaymentRequest request, Guid userId);
}