using TradingAlpha.Domain.Entities;

namespace TradingAlpha.Domain.Interfaces;

/// <summary>
/// Repository abstraction for User data access.
/// 
/// Application layer depends on this interface.
/// Infrastructure layer implements it with EF Core.
/// This is the Dependency Inversion Principle in action —
/// business logic never touches the database directly.
/// </summary>
public interface IUserRepository
{
    /// <summary>Find a user by their unique ID</summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Find a user by username (case-insensitive)</summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>Check if a username is already taken</summary>
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>Add a new user to the data store (call SaveChangesAsync separately)</summary>
    Task AddAsync(User user, CancellationToken ct = default);
}