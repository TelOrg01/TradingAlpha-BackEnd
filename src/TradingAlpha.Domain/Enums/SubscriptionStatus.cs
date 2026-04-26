namespace TradingAlpha.Domain.Entities;

/// <summary>
/// Represents the lifecycle states of a user subscription.
/// Transitions: Active → Expired (automatic) or Active → Cancelled (user-initiated).
/// </summary>
public enum SubscriptionStatus
{
    Active,
    Expired,
    Cancelled
}