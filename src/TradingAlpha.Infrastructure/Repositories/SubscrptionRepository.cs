using Microsoft.EntityFrameworkCore;
using TradingAlpha.Domain.Entities;
using TradingAlpha.Domain.Interfaces;
using TradingAlpha.Infrastructure.Data;

namespace TradingAlpha.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ISubscriptionRepository.
/// Follows the same pattern as UserRepository — thin async wrappers over DbSet queries.
/// </summary>
public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;

    public SubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the most recent active subscription for a user.
    /// "Active" = Status is Active AND ExpiryDate hasn't passed yet.
    /// Orders by CreatedAt descending so if somehow multiple active subs exist,
    /// the latest one wins.
    /// </summary>
    public async Task<Subscription?> GetActiveByUserIdAsync(Guid userId)
    {
        return await _context.Subscriptions
            .Where(s => s.UserId == userId
                        && s.Status == SubscriptionStatus.Active
                        && s.ExpiryDate > DateTime.UtcNow)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Subscription?> GetByIdAsync(Guid subscriptionId)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);
    }

    /// <summary>
    /// Finds a subscription by Razorpay order ID.
    /// Used during payment verification to locate the matching subscription.
    /// </summary>
    public async Task<Subscription?> GetByOrderIdAsync(string orderId)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.OrderId == orderId);
    }

    public async Task AddAsync(Subscription subscription)
    {
        await _context.Subscriptions.AddAsync(subscription);
    }
}