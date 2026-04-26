namespace TradingAlpha.Domain.Entities;

/// <summary>
/// Represents the lifecycle states of a payment transaction.
/// Pending → Success (verified) or Pending → Failed (verification failed / gateway error).
/// Refunded is set when a refund is processed post-success.
/// </summary>
public enum PaymentStatus
{
    Pending,
    Success,
    Failed,
    Refunded
}