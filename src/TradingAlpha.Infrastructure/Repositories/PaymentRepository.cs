using Microsoft.EntityFrameworkCore;
using TradingAlpha.Domain.Entities;
using TradingAlpha.Domain.Interfaces;
using TradingAlpha.Infrastructure.Data;

namespace TradingAlpha.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IPaymentRepository.
/// PaymentRecords are an audit log — mostly inserts and single-field updates.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Finds a payment record by Razorpay order ID.
    /// Used during verification to locate the Pending record created at order time.
    /// </summary>
    public async Task<PaymentRecord?> GetByOrderIdAsync(string gatewayOrderId)
    {
        return await _context.PaymentRecords
            .FirstOrDefaultAsync(p => p.GatewayOrderId == gatewayOrderId);
    }

    /// <summary>
    /// Gets all payment records for a user, most recent first.
    /// Used for payment history display on the profile page.
    /// </summary>
    public async Task<List<PaymentRecord>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PaymentRecords
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(PaymentRecord paymentRecord)
    {
        await _context.PaymentRecords.AddAsync(paymentRecord);
    }
}