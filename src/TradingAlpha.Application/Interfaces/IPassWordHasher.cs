namespace TradingAlpha.Application.Interfaces;

/// <summary>
/// Password hashing abstraction.
/// 
/// Decouples the hashing algorithm (BCrypt, Argon2, etc.) from
/// the business logic. Infrastructure provides the implementation.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hash a plain-text password</summary>
    string Hash(string password);

    /// <summary>Verify a plain-text password against a stored hash</summary>
    bool Verify(string password, string hash);
}