namespace TradingAlpha.Application.Interfaces;

/// <summary>
/// Abstraction over the Razorpay payment gateway SDK.
/// Lives in Application layer so PaymentService can use it without
/// depending on the Razorpay NuGet package directly.
/// Infrastructure layer provides the implementation.
/// </summary>
public interface IRazorpayService
{
    /// <summary>
    /// Creates a Razorpay order via their Orders API.
    /// Returns the order ID and amount in paise.
    /// </summary>
    /// <param name="amountInRupees">Plan price in rupees (e.g., 3999)</param>
    /// <param name="currency">ISO 4217 code (e.g., "INR")</param>
    /// <param name="receipt">Unique receipt ID for tracking (e.g., "rcpt_guid")</param>
    /// <returns>Tuple of (Razorpay Order ID, Amount in Paise)</returns>
    Task<(string OrderId, long AmountInPaise)> CreateOrderAsync(
        decimal amountInRupees,
        string currency,
        string receipt);

    /// <summary>
    /// Verifies the payment signature using HMAC-SHA256.
    /// Compares: HMAC_SHA256(orderId + "|" + paymentId, keySecret) == signature
    /// </summary>
    /// <returns>True if signature is valid (payment is authentic)</returns>
    bool VerifyPaymentSignature(string orderId, string paymentId, string signature);

    /// <summary>
    /// Returns the Razorpay public Key ID for the frontend checkout modal.
    /// </summary>
    string GetKeyId();
}