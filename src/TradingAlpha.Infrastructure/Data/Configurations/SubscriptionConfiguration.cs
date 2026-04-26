using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingAlpha.Domain.Entities;

namespace TradingAlpha.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the Subscriptions table.
/// Follows the same pattern as UserConfiguration — explicit column types,
/// string-stored enums, and proper foreign key relationships.
/// </summary>
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.PlanId)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.PlanName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Amount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(s => s.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("INR");

        // Store enum as string for readability in DB queries (same as UserRole)
        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(s => s.PaymentMethod)
            .HasMaxLength(20);

        builder.Property(s => s.PaymentId)
            .HasMaxLength(100);

        builder.Property(s => s.OrderId)
            .HasMaxLength(100);

        builder.Property(s => s.StartDate)
            .IsRequired();

        builder.Property(s => s.ExpiryDate)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Foreign key: each subscription belongs to one user
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on UserId — frequent lookup: "get active subscription for this user"
        builder.HasIndex(s => s.UserId);

        // Index on OrderId — lookup during payment verification
        builder.HasIndex(s => s.OrderId);
    }
}