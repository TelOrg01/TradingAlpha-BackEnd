namespace TradingAlpha.Contracts.Payment;

/// <summary>
/// Response returned after verifying a Razorpay payment and activating the subscription.
/// The frontend uses this to update the UI and navigate to the profile page.
/// </summary>
public record VerifyPaymentResponse(
    bool Success,
    Guid SubscriptionId,
    string PlanName,
    DateTime ExpiryDate,
    string Message          // "Subscription activated successfully"
);