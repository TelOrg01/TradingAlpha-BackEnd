namespace TradingAlpha.Contracts.Payment;

/// <summary>
/// Request to verify a completed Razorpay payment.
/// These three values come directly from the Razorpay checkout handler callback.
/// The backend uses them to verify the HMAC-SHA256 signature and activate the subscription.
/// </summary>
public record VerifyPaymentRequest(
    string OrderId,         // razorpay_order_id from checkout response
    string PaymentId,       // razorpay_payment_id from checkout response
    string Signature        // razorpay_signature from checkout response
);