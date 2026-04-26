using TradingAlpha.Application.Interfaces;

namespace TradingAlpha.Infrastructure.Services;

/// <summary>
/// PasswordHasher — Wraps BCrypt for secure password hashing.
/// 
/// Implements IPasswordHasher (defined in Application layer).
/// BCrypt automatically handles salting — each hash is unique
/// even for identical passwords.
/// 
/// Work factor 12 is the current recommendation (2024-2025).
/// Increase if hardware gets faster; decrease if login is too slow.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>BCrypt work factor — higher = slower = more secure</summary>
    private const int WorkFactor = 12;

    /// <inheritdoc />
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <inheritdoc />
    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}