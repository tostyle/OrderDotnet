using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Configurations;

/// <summary>
/// Entity configuration for OrderStock entity
/// </summary>
public class OrderStockConfiguration : IEntityTypeConfiguration<OrderStock>
{
    public void Configure(EntityTypeBuilder<OrderStock> builder)
    {
        builder.ToTable("OrderStockReservations");

        // Primary key
        builder.HasKey(s => s.Id);

        // Configure StockReservationId value object
        builder.Property(s => s.Id)
            .HasConversion(
                reservationId => reservationId.Value,
                value => StockReservationId.From(value))
            .IsRequired();

        // Configure OrderId value object
        builder.Property(s => s.OrderId)
            .HasConversion(
                orderId => orderId.Value,
                value => OrderId.From(value))
            .IsRequired();

        // Configure ProductId value object
        builder.Property(s => s.ProductId)
            .HasConversion(
                productId => productId.Value,
                value => ProductId.From(value))
            .IsRequired();

        // Configure QuantityReserved
        builder.Property(s => s.QuantityReserved)
            .IsRequired();

        // Configure ReservationDate
        builder.Property(s => s.ReservationDate)
            .IsRequired();

        // Configure ExpirationDate as optional
        builder.Property(s => s.ExpirationDate)
            .IsRequired(false);

        // Configure Status enum
        builder.Property(s => s.Status)
            .HasConversion<string>()
            .IsRequired();

        // Configure ExternalReservationId as optional
        builder.Property(s => s.ExternalReservationId)
            .HasMaxLength(255)
            .IsRequired(false);

        // Configure Notes as optional
        builder.Property(s => s.Notes)
            .HasMaxLength(1000)
            .IsRequired(false);

        // Indexes for better performance
        builder.HasIndex(s => s.OrderId);
        builder.HasIndex(s => s.ProductId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.ReservationDate);
        builder.HasIndex(s => s.ExpirationDate);
        builder.HasIndex(s => s.ExternalReservationId);
    }
}
