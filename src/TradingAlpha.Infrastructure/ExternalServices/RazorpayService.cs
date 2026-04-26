using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Razorpay.Api;
using TradingAlpha.Application.Interfaces;

namespace TradingAlpha.Infrastructure.ExternalServices;

/// <summary>
/// Razorpay SDK wrapper. All Razorpay-specific code is isolated here.
/// If you ever switch to Stripe or another gateway, only this class changes.
/// 
/// Uses the official Razorpay .NET SDK (NuGet: Razorpay v3.3.2+).
/// SDK handles Basic Auth (KeyId:KeySecret) for API calls internally.
/// </summary>
public class RazorpayService : IRazorpayService
{
    private readonly RazorpayClient _client;
    private readonly RazorpaySettings _settings;
    private readonly ILogger<RazorpayService> _logger;

    public RazorpayService(
        IOptions<RazorpaySettings> settings,
        ILogger<RazorpayService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // RazorpayClient authenticates all API calls with KeyId + KeySecret (Basic Auth)
        _client = new RazorpayClient(_settings.KeyId, _settings.KeySecret);
    }

    /// <summary>
    /// Creates a Razorpay order via POST /v1/orders.
    /// The returned order_id is required to open the checkout modal on the frontend.
    /// Amount is converted from rupees to paise (Razorpay expects smallest currency unit).
    /// </summary>
    public async Task<(string OrderId, long AmountInPaise)> CreateOrderAsync(
        decimal amountInRupees,
        string currency,
        string receipt)
    {
        // Convert rupees to paise (₹3,999 → 399900 paise)
        var amountInPaise = (long)(amountInRupees * 100);

        var options = new Dictionary<string, object>
        {
            { "amount", amountInPaise },
            { "currency", currency },
            { "receipt", receipt }
        };

        _logger.LogInformation(
            "Creating Razorpay order: Amount={Amount} paise, Currency={Currency}, Receipt={Receipt}",
            amountInPaise, currency, receipt);

        // SDK call — this hits Razorpay's API synchronously (SDK limitation)
        // Wrapped in Task.Run to avoid blocking the request thread
        var order = await Task.Run(() => _client.Order.Create(options));

        // Razorpay SDK returns dynamic objects — cast to string to avoid CS1973
        // (extension methods like LogInformation don't work with dynamic arguments)
        string orderId = order["id"].ToString();

        _logger.LogInformation("Razorpay order created: OrderId={OrderId}", orderId);

        return (orderId, amountInPaise);
    }

    /// <summary>
    /// Verifies payment authenticity using HMAC-SHA256 signature comparison.
    /// Formula: HMAC_SHA256(orderId + "|" + paymentId, keySecret) == signature
    /// 
    /// Uses the Razorpay SDK's built-in Utils.verifyPaymentSignature() method
    /// which handles the HMAC computation internally.
    /// </summary>
    public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
    {
        try
        {
            var attributes = new Dictionary<string, string>
            {
                { "razorpay_order_id", orderId },
                { "razorpay_payment_id", paymentId },
                { "razorpay_signature", signature }
            };

            Utils.verifyPaymentSignature(attributes);

            _logger.LogInformation(
                "Payment signature verified: OrderId={OrderId}, PaymentId={PaymentId}",
                orderId, paymentId);

            return true;
        }
        catch (Razorpay.Api.Errors.SignatureVerificationError ex)
        {
            string errorMessage = ex.Message;

            _logger.LogWarning(
                "Payment signature verification FAILED: OrderId={OrderId}, PaymentId={PaymentId}, Error={Error}",
                orderId, paymentId, errorMessage);

            return false;
        }
    }

    /// <summary>
    /// Returns the public Key ID for frontend checkout configuration.
    /// This is safe to expose — it's not the secret.
    /// </summary>
    public string GetKeyId() => _settings.KeyId;
}