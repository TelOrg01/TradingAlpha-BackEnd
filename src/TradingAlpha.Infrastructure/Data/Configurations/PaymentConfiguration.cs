using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingAlpha.Domain.Entities;

namespace TradingAlpha.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the PaymentHistory table.
/// This is an append-only audit log — records are created as Pending
/// and updated once to Success/Failed during verification. Never deleted.
/// </summary>
public class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
{
    public void Configure(EntityTypeBuilder<PaymentRecord> builder)
    {
        builder.ToTable("PaymentHistory");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("INR");

        // Store enum as string for readability (same pattern as SubscriptionStatus)
        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(p => p.PaymentMethod)
            .HasMaxLength(20);

        builder.Property(p => p.GatewayOrderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.GatewayPaymentId)
            .HasMaxLength(100);

        builder.Property(p => p.GatewaySignature)
            .HasMaxLength(200);

        // Full gateway response JSON — no max length constraint, stored as TEXT
        builder.Property(p => p.RawResponse)
            .HasColumnType("text");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Foreign key to Users
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to Subscriptions (nullable — Pending payments have no subscription yet)
        builder.HasOne(p => p.Subscription)
            .WithMany()
            .HasForeignKey(p => p.SubscriptionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Index on GatewayOrderId — lookup during verification
        builder.HasIndex(p => p.GatewayOrderId);

        // Index on UserId — payment history listing
        builder.HasIndex(p => p.UserId);
    }
}