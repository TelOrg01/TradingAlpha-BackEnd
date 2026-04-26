namespace TradingAlpha.Contracts.Payment;

/// <summary>
/// Response returned after successfully creating a Razorpay order.
/// Contains everything the Angular frontend needs to open the Razorpay checkout modal.
/// </summary>
public record CreateOrderResponse(
    string OrderId,         // Razorpay order ID (e.g., "order_9A33XWu170gUtm")
    long AmountInPaise,     // Amount in smallest currency unit (₹3,999 = 399900)
    string Currency,        // "INR"
    string GatewayKeyId,    // Razorpay public Key ID (rzp_test_...) — safe for client
    string PlanName,        // "Pro" — displayed in checkout modal
    string UserName,        // Prefill for checkout — better conversion
    string UserEmail        // Prefill for checkout — better conversion
);