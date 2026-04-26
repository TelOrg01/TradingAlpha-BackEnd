using TradingAlpha.Domain.Entities;

namespace TradingAlpha.Domain.Interfaces;

/// <summary>
/// Repository contract for Subscription persistence.
/// Follows the same pattern as IUserRepository — thin async methods,
/// no EF-specific types leak into the domain.
/// </summary>
public interface ISubscriptionRepository
{
    /// <summary>
    /// Gets the currently active subscription for a user.
    /// Returns null if the user has no active subscription.
    /// "Active" means Status == Active AND ExpiryDate > UTC now.
    /// </summary>
    Task<Subscription?> GetActiveByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets a subscription by its primary key.
    /// Used when linking a PaymentRecord to a Subscription after verification.
    /// </summary>
    Task<Subscription?> GetByIdAsync(Guid subscriptionId);

    /// <summary>
    /// Gets a subscription by its Razorpay order ID.
    /// Used during payment verification to find the matching subscription.
    /// </summary>
    Task<Subscription?> GetByOrderIdAsync(string orderId);

    /// <summary>
    /// Adds a new subscription record to the context.
    /// Call IUnitOfWork.SaveChangesAsync() to persist.
    /// </summary>
    Task AddAsync(Subscription subscription);
}