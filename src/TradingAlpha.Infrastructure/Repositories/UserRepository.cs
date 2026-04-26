using Microsoft.EntityFrameworkCore;
using TradingAlpha.Domain.Entities;
using TradingAlpha.Domain.Interfaces;
using TradingAlpha.Infrastructure.Data;

namespace TradingAlpha.Infrastructure.Repositories;

/// <summary>
/// UserRepository — Implements IUserRepository using EF Core.
/// 
/// This is the ONLY place that contains EF queries for the User entity.
/// The Application layer calls the interface methods; it never sees
/// DbContext, LINQ, or SQL.
/// 
/// All queries use CancellationToken for proper async cancellation.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username, ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
    }
}