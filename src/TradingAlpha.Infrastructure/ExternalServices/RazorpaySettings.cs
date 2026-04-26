namespace TradingAlpha.Infrastructure.ExternalServices;

/// <summary>
/// POCO mapped to the "RazorpaySettings" section in appsettings.json.
/// Follows the same Options pattern as JwtSettings.
/// 
/// KeyId is public (sent to frontend for checkout modal).
/// KeySecret is private (used only server-side for order creation and signature verification).
/// </summary>
public class RazorpaySettings
{
    public const string SectionName = "RazorpaySettings";

    /// <summary>
    /// Razorpay Key ID (e.g., "rzp_test_XXXXXXXXXXXX").
    /// Public — safe to send to the frontend.
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// Razorpay Key Secret. PRIVATE — never expose to the frontend.
    /// Used for creating orders (Basic Auth) and verifying payment signatures (HMAC-SHA256).
    /// </summary>
    public string KeySecret { get; set; } = string.Empty;
}