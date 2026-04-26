namespace TradingAlpha.Domain.Entities;

/// <summary>
/// Represents a user's subscription to a TradingAlpha plan.
/// Each successful payment creates one Subscription record.
/// A user can have multiple subscriptions over time (renewals, upgrades),
/// but only the latest active one determines their access level.
/// </summary>
public class Subscription
{
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the Users table. Each subscription belongs to exactly one user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Plan identifier from the plan catalog (e.g., "starter", "pro", "lifetime").
    /// Stored as a string so the plan catalog can evolve without migrations.
    /// </summary>
    public string PlanId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable plan name at time of purchase (e.g., "Pro").
    /// Stored separately because plan names could change in the future,
    /// and we want the historical record to reflect what the user actually bought.
    /// </summary>
    public string PlanName { get; set; } = string.Empty;

    /// <summary>
    /// Amount paid in the base currency unit (e.g., 3999.00 for ₹3,999).
    /// NOT in paise — conversion to paise happens only in the Razorpay service layer.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// ISO 4217 currency code (e.g., "INR").
    /// </summary>
    public string Currency { get; set; } = "INR";

    public SubscriptionStatus Status { get; set; }

    /// <summary>
    /// Payment method used (e.g., "Card", "UPI", "NetBanking", "Wallet").
    /// Populated after payment verification from the gateway response.
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Razorpay payment ID returned after successful payment (e.g., "pay_29QQoUBi66xm2f").
    /// Used for refund requests and payment tracking on the Razorpay dashboard.
    /// </summary>
    public string? PaymentId { get; set; }

    /// <summary>
    /// Razorpay order ID created during the create-order step (e.g., "order_9A33XWu170gUtm").
    /// Links this subscription back to the specific Razorpay order.
    /// </summary>
    public string? OrderId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation property — links back to the User entity
    public User User { get; set; } = null!;
}