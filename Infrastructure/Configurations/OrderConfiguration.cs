using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Configurations;

/// <summary>
/// Entity configuration for Order entity
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        // Primary key
        builder.HasKey(o => o.Id);

        // Configure OrderId value object
        builder.Property(o => o.Id)
            .HasConversion(
                orderId => orderId.Value,
                value => OrderId.From((Guid)value))
            .IsRequired();

        // Configure timestamps
        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .IsRequired();

        // Configure version for optimistic concurrency
        builder.Property(o => o.Version)
            .IsConcurrencyToken()
            .IsRequired();

        // Configure WorkflowId as optional
        builder.Property(o => o.WorkflowId)
            .HasMaxLength(255)
            .IsRequired(false);

        // Configure ReferenceId
        builder.Property(o => o.ReferenceId)
            .HasMaxLength(255)
            .IsRequired();

        // Configure navigation properties

        // One-to-many relationship with OrderLoyalty
        builder.HasMany(o => o.LoyaltyTransactions)
            .WithOne(l => l.Order)
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many relationship with OrderStock
        builder.HasMany(o => o.StockReservations)
            .WithOne(s => s.Order)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many relationship with OrderItems
        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-one relationship with OrderPayment
        builder.HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<OrderPayment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for better performance
        builder.HasIndex(o => o.WorkflowId);
        builder.HasIndex(o => o.ReferenceId)
            .IsUnique(); // ReferenceId should be unique
        builder.HasIndex(o => o.CreatedAt);
    }
}
