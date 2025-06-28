using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Configurations;

/// <summary>
/// Entity configuration for OrderLoyalty entity
/// </summary>
public class OrderLoyaltyConfiguration : IEntityTypeConfiguration<OrderLoyalty>
{
    public void Configure(EntityTypeBuilder<OrderLoyalty> builder)
    {
        builder.ToTable("OrderLoyaltyTransactions");

        // Primary key
        builder.HasKey(l => l.Id);

        // Configure LoyaltyTransactionId value object
        builder.Property(l => l.Id)
            .HasConversion(
                transactionId => transactionId.Value,
                value => LoyaltyTransactionId.From(value))
            .IsRequired();

        // Configure OrderId value object
        builder.Property(l => l.OrderId)
            .HasConversion(
                orderId => orderId.Value,
                value => OrderId.From(value))
            .IsRequired();

        // Configure TransactionType enum
        builder.Property(l => l.TransactionType)
            .HasConversion<string>()
            .IsRequired();

        // Configure Points
        builder.Property(l => l.Points)
            .IsRequired();

        // Configure TransactionDate
        builder.Property(l => l.TransactionDate)
            .IsRequired();

        // Configure Description
        builder.Property(l => l.Description)
            .HasMaxLength(500)
            .IsRequired();

        // Configure ExternalTransactionId as optional
        builder.Property(l => l.ExternalTransactionId)
            .HasMaxLength(255)
            .IsRequired(false);

        // Indexes for better performance
        builder.HasIndex(l => l.OrderId);
        builder.HasIndex(l => l.TransactionType);
        builder.HasIndex(l => l.TransactionDate);
        builder.HasIndex(l => l.ExternalTransactionId);
    }
}
