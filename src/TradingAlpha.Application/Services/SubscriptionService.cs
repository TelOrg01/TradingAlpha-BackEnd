using TradingAlpha.Application.Interfaces;
using TradingAlpha.Contracts.Payment;
using TradingAlpha.Domain.Common;
using TradingAlpha.Domain.Interfaces;

namespace TradingAlpha.Application.Services;

/// <summary>
/// Handles subscription status queries.
/// Kept separate from PaymentService for single responsibility —
/// PaymentService handles the payment lifecycle,
/// SubscriptionService handles subscription reads.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public SubscriptionService(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    /// <summary>
    /// Gets the current subscription for a user.
    /// If no active subscription exists, returns a response with IsActive=false
    /// instead of an error — the profile page always renders, just with different content.
    /// </summary>
    public async Task<Result<SubscriptionResponse>> GetCurrentSubscriptionAsync(Guid userId)
    {
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId);

        // No active subscription — return an "inactive" response, not an error.
        // The frontend uses IsActive to toggle between "Subscribe now" and plan details.
        if (subscription is null)
        {
            return Result<SubscriptionResponse>.Success(new SubscriptionResponse(
                PlanId: string.Empty,
                PlanName: "None",
                Status: "Inactive",
                IsActive: false,
                DaysRemaining: 0,
                ExpiryDate: null,
                StartDate: null
            ));
        }

        // Calculate days remaining (floor to 0 if somehow negative)
        var daysRemaining = Math.Max(0,
            (int)(subscription.ExpiryDate - DateTime.UtcNow).TotalDays);

        return Result<SubscriptionResponse>.Success(new SubscriptionResponse(
            PlanId: subscription.PlanId,
            PlanName: subscription.PlanName,
            Status: subscription.Status.ToString(),
            IsActive: true,
            DaysRemaining: daysRemaining,
            ExpiryDate: subscription.ExpiryDate,
            StartDate: subscription.StartDate
        ));
    }
}