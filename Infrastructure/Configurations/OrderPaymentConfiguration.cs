using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Configurations;

/// <summary>
/// Entity configuration for OrderPayment entity
/// </summary>
public class OrderPaymentConfiguration : IEntityTypeConfiguration<OrderPayment>
{
    public void Configure(EntityTypeBuilder<OrderPayment> builder)
    {
        builder.ToTable("OrderPayments");

        // Primary key
        builder.HasKey(p => p.Id);

        // Configure PaymentId value object
        builder.Property(p => p.Id)
            .HasConversion(
                paymentId => paymentId.Value,
                value => PaymentId.From(value))
            .IsRequired();

        // Configure OrderId value object
        builder.Property(p => p.OrderId)
            .HasConversion(
                orderId => orderId.Value,
                value => OrderId.From(value))
            .IsRequired();

        // Configure PaymentMethod value object
        builder.Property(p => p.PaymentMethod)
            .HasConversion(
                pm => pm.ToString(),
                value => PaymentMethod.CreditCard("0000", "Unknown")) // Default conversion - this should be improved
            .HasMaxLength(50)
            .IsRequired();

        // Configure Amount
        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        // Configure Currency
        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .IsRequired();

        // Configure PaidDate as optional
        builder.Property(p => p.PaidDate)
            .IsRequired(false);

        // Configure Status enum
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .IsRequired();

        // Configure TransactionReference as optional
        builder.Property(p => p.TransactionReference)
            .HasMaxLength(255)
            .IsRequired(false);

        // Configure Notes as optional
        builder.Property(p => p.Notes)
            .HasMaxLength(1000)
            .IsRequired(false);

        // Indexes for better performance
        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.TransactionReference);
    }
}
