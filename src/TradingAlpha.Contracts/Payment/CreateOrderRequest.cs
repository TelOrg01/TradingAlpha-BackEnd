namespace TradingAlpha.Contracts.Payment;

/// <summary>
/// Request to create a Razorpay order for a specific plan.
/// Only the planId is sent from the client — amount is ALWAYS resolved server-side
/// from the plan catalog to prevent price tampering.
/// </summary>
public record CreateOrderRequest(
    string PlanId   // "starter", "pro", or "lifetime"
);