using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Configurations;

/// <summary>
/// Entity configuration for OrderItem entity
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", t =>
        {
            t.HasCheckConstraint("CK_OrderItems_Quantity", "Quantity > 0");
            t.HasCheckConstraint("CK_OrderItems_NetAmount", "NetAmount >= 0");
            t.HasCheckConstraint("CK_OrderItems_GrossAmount", "GrossAmount >= 0");
        });

        // Primary key
        builder.HasKey(oi => oi.Id);

        // Configure OrderItemId value object
        builder.Property(oi => oi.Id)
            .HasConversion(
                orderItemId => orderItemId.Value,
                value => OrderItemId.From(value))
            .IsRequired();

        // Configure OrderId value object
        builder.Property(oi => oi.OrderId)
            .HasConversion(
                orderId => orderId.Value,
                value => OrderId.From(value))
            .IsRequired();

        // Configure ProductId value object
        builder.Property(oi => oi.ProductId)
            .HasConversion(
                productId => productId.Value,
                value => ProductId.From(value))
            .IsRequired();

        // Configure Quantity
        builder.Property(oi => oi.Quantity)
            .IsRequired();

        // Configure NetAmount with precision
        builder.Property(oi => oi.NetAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        // Configure GrossAmount with precision
        builder.Property(oi => oi.GrossAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        // Configure Currency with default value
        builder.Property(oi => oi.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("THB")
            .IsRequired();

        // Indexes for better performance
        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.ProductId);
        builder.HasIndex(oi => new { oi.OrderId, oi.ProductId })
            .HasDatabaseName("IX_OrderItems_OrderId_ProductId");
    }
}
