using TradingAlpha.Domain.Entities;

namespace TradingAlpha.Domain.Interfaces;

/// <summary>
/// Repository contract for PaymentRecord persistence.
/// PaymentRecords are an append-only audit log — updates are limited to
/// status transitions (Pending → Success/Failed) during verification.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Finds a payment record by Razorpay order ID.
    /// Used during verification to locate the Pending record created at order time.
    /// </summary>
    Task<PaymentRecord?> GetByOrderIdAsync(string gatewayOrderId);

    /// <summary>
    /// Gets all payment records for a user, ordered by most recent first.
    /// Used for displaying payment history on the profile page.
    /// </summary>
    Task<List<PaymentRecord>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Adds a new payment record to the context.
    /// Call IUnitOfWork.SaveChangesAsync() to persist.
    /// </summary>
    Task AddAsync(PaymentRecord paymentRecord);
}