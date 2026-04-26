using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingAlpha.Domain.Entities;

namespace TradingAlpha.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the User entity.
/// 
/// Defines table name, column types, constraints, and indexes.
/// Kept separate from the entity class — the domain entity stays
/// clean and framework-agnostic.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // Primary key
        builder.HasKey(u => u.Id);

        // Username: required, unique, max 50 chars
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.Username)
            .IsUnique();

        // Name: required, max 100 chars
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        // PasswordHash: required, stored as text (BCrypt hashes are ~60 chars)
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(200);

        // Role: stored as string instead of int for readability in DB
        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Timestamps
        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.UpdatedAt)
            .IsRequired(false);
    }
}