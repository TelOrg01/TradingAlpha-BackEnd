namespace TradingAlpha.Domain.Entities;

/// <summary>
/// User domain entity — represents a registered user in the system.
/// 
/// This is the core domain model. It has NO dependency on EF, DTOs, or
/// any external framework. EF maps to this via Fluent API configuration
/// in Infrastructure layer (UserConfiguration.cs).
/// 
/// Password is stored as a BCrypt hash — never plain text.
/// </summary>
public class User
{
    public Guid Id { get; set; }

    /// <summary>Unique login identifier — always stored lowercase</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Display name shown in UI</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>BCrypt hashed password — never store or return plain text</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>User role for authorization (User, Admin)</summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>UTC timestamp of account creation</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of last profile update (nullable)</summary>
    public DateTime? UpdatedAt { get; set; }
}