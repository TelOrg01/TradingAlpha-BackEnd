namespace TradingAlpha.Contracts.Payment;

/// <summary>
/// Response for GET /api/subscription/current.
/// Contains everything the profile page needs to display subscription status.
/// </summary>
public record SubscriptionResponse(
    string PlanId,          // "starter", "pro", "lifetime"
    string PlanName,        // "Starter", "Pro", "Lifetime"
    string Status,          // "Active", "Expired", "Cancelled"
    bool IsActive,          // Quick boolean check for download gating
    int DaysRemaining,      // 0 if expired — frontend shows renewal prompt
    DateTime? ExpiryDate,   // Null only if no subscription exists
    DateTime? StartDate     // When the current subscription period began
);