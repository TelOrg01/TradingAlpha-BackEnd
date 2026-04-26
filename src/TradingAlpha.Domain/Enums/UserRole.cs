namespace TradingAlpha.Domain.Entities;

/// <summary>
/// User roles for role-based authorization.
/// Stored as string in the database via EF configuration.
/// Included as a claim in the JWT token.
/// </summary>
public enum UserRole
{
    User,
    Admin
}