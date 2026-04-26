using Microsoft.EntityFrameworkCore;
using TradingAlpha.Domain.Entities;
using TradingAlpha.Domain.Interfaces;

namespace TradingAlpha.Infrastructure.Data;

/// <summary>
/// EF Core DbContext — the bridge between domain entities and PostgreSQL.
/// 
/// Also implements IUnitOfWork so the Application layer can call
/// SaveChangesAsync() without depending on EF directly.
/// 
/// Entity configurations are in separate files (UserConfiguration.cs)
/// to keep this class clean as more entities are added.
/// </summary>
public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>Users table</summary>
    public DbSet<User> Users => Set<User>();

    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<PaymentRecord> PaymentRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// IUnitOfWork implementation — delegates to EF's SaveChangesAsync.
    /// This way Application layer calls _unitOfWork.SaveChangesAsync()
    /// without knowing about DbContext.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Auto-set UpdatedAt on modified entities
        foreach (var entry in ChangeTracker.Entries<User>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(ct);
    }
}