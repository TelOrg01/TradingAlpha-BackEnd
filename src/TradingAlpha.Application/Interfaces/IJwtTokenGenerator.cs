using TradingAlpha.Domain.Entities;

namespace TradingAlpha.Application.Interfaces;

/// <summary>
/// JWT token generation abstraction.
/// 
/// Application layer defines WHAT it needs (generate a token for a user).
/// Infrastructure layer provides HOW (signing key, claims, expiry).
/// This keeps cryptographic concerns out of the business logic.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>Generate a signed JWT for the given user</summary>
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}