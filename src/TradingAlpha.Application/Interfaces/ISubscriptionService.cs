using TradingAlpha.Contracts.Payment;
using TradingAlpha.Domain.Common;

namespace TradingAlpha.Application.Interfaces;

/// <summary>
/// Service for querying subscription status.
/// Used by the profile page and (later) the WPF desktop app's license check.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Gets the current subscription status for a user.
    /// Returns a response indicating active/inactive with days remaining.
    /// </summary>
    Task<Result<SubscriptionResponse>> GetCurrentSubscriptionAsync(Guid userId);
}