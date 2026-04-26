namespace TradingAlpha.Domain.Entities;

/// <summary>
/// Immutable audit record for every payment attempt (successful or failed).
/// Unlike Subscription (which tracks current state), PaymentRecord is an append-only log.
/// Every create-order call creates a Pending record; verification updates it to Success/Failed.
/// This ensures we have a complete trail even for abandoned or failed payments.
/// </summary>
public class PaymentRecord
{
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the Users table. The user who initiated this payment.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Foreign key to the Subscriptions table.
    /// Null while payment is Pending; set only after successful verification
    /// when the corresponding Subscription record is created.
    /// </summary>
    public Guid? SubscriptionId { get; set; }

    /// <summary>
    /// Amount in base currency unit (e.g., 3999.00 for ₹3,999).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// ISO 4217 currency code (e.g., "INR").
    /// </summary>
    public string Currency { get; set; } = "INR";

    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Payment method used (e.g., "Card", "UPI", "NetBanking").
    /// Null for Pending payments (method isn't known until the user completes checkout).
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Razorpay order ID (e.g., "order_9A33XWu170gUtm").
    /// Set immediately when the order is created — available for all payment states.
    /// </summary>
    public string GatewayOrderId { get; set; } = string.Empty;

    /// <summary>
    /// Razorpay payment ID (e.g., "pay_29QQoUBi66xm2f").
    /// Set only after the user completes payment in the Razorpay modal.
    /// </summary>
    public string? GatewayPaymentId { get; set; }

    /// <summary>
    /// Razorpay signature returned by checkout for server-side HMAC verification.
    /// Stored for audit purposes — proof that the payment was verified.
    /// </summary>
    public string? GatewaySignature { get; set; }

    /// <summary>
    /// Full JSON response from Razorpay stored as-is for debugging and dispute resolution.
    /// Never parsed in application logic — treat as an opaque audit blob.
    /// </summary>
    public string? RawResponse { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Subscription? Subscription { get; set; }
}