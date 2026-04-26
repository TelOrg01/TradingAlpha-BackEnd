namespace TradingAlpha.Application.Common;

/// <summary>
/// Server-side plan catalog — the single source of truth for plan pricing.
/// Amount is ALWAYS resolved from here during order creation, never from the client.
/// This prevents price tampering (e.g., user sends amount=1 via browser dev tools).
/// 
/// If you later move plans to a database table, replace this class with a
/// repository call — the rest of the payment flow stays unchanged.
/// </summary>
public static class PlanCatalog
{
    /// <summary>
    /// Represents a subscription plan with its pricing and duration.
    /// </summary>
    public record PlanInfo(
        string Id,
        string Name,
        decimal PriceInRupees,
        int DurationInDays,
        string Description
    );

    /// <summary>
    /// All available plans. Keyed by plan ID for O(1) lookup.
    /// </summary>
    private static readonly Dictionary<string, PlanInfo> Plans = new(StringComparer.OrdinalIgnoreCase)
    {
        ["starter"] = new PlanInfo("Starter", "Starter", 4m, 30, "Monthly access"),
        ["pro"] = new PlanInfo("Pro", "Pro", 5m, 365, "Annual access"),
        ["lifetime"] = new PlanInfo("Lifetime", "Lifetime", 6m, 36500, "Lifetime access")
    };

    /// <summary>
    /// Gets a plan by ID. Returns null if the plan doesn't exist.
    /// </summary>
    public static PlanInfo? GetPlan(string planId)
    {
        Plans.TryGetValue(planId, out var plan);
        return plan;
    }

    /// <summary>
    /// Checks if a plan ID is valid.
    /// </summary>
    public static bool Exists(string planId)
    {
        return Plans.ContainsKey(planId);
    }

    /// <summary>
    /// Gets all available plans. Used for listing plans if needed.
    /// </summary>
    public static IReadOnlyCollection<PlanInfo> GetAll()
    {
        return Plans.Values.ToList().AsReadOnly();
    }
}