using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Configurations;

/// <summary>
/// Entity configuration for OrderJourney entity
/// Configures the order state transition audit trail with compound indexes
/// </summary>
public class OrderJourneyConfiguration : IEntityTypeConfiguration<OrderJourney>
{
    public void Configure(EntityTypeBuilder<OrderJourney> builder)
    {
        builder.ToTable("OrderJourneys");

        // Primary key
        builder.HasKey(j => j.Id);

        // Configure OrderJourneyId value object
        builder.Property(j => j.Id)
            .HasConversion(
                journeyId => journeyId.Value,
                value => OrderJourneyId.From(value))
            .IsRequired();

        // Configure OrderId value object
        builder.Property(j => j.OrderId)
            .HasConversion(
                orderId => orderId.Value,
                value => OrderId.From(value))
            .IsRequired();

        // Configure OrderState enums
        builder.Property(j => j.OldState)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(j => j.NewState)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Configure TransitionDate
        builder.Property(j => j.TransitionDate)
            .IsRequired();

        // Configure CreatedAt
        builder.Property(j => j.CreatedAt)
            .IsRequired();

        // Configure optional fields
        builder.Property(j => j.Reason)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(j => j.InitiatedBy)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(j => j.Metadata)
            .HasColumnType("text")
            .IsRequired(false);

        // Compound indexes as specified in the requirements
        // Index for OrderId + NewState for efficient querying of orders by their current/target state
        builder.HasIndex(j => new { j.OrderId, j.NewState })
            .HasDatabaseName("IX_OrderJourneys_OrderId_NewState");

        // Index for OrderId + OldState for efficient querying of orders by their previous state
        builder.HasIndex(j => new { j.OrderId, j.OldState })
            .HasDatabaseName("IX_OrderJourneys_OrderId_OldState");

        // Additional performance indexes
        builder.HasIndex(j => j.OrderId)
            .HasDatabaseName("IX_OrderJourneys_OrderId");

        builder.HasIndex(j => j.TransitionDate)
            .HasDatabaseName("IX_OrderJourneys_TransitionDate");

        builder.HasIndex(j => j.NewState)
            .HasDatabaseName("IX_OrderJourneys_NewState");

        builder.HasIndex(j => j.OldState)
            .HasDatabaseName("IX_OrderJourneys_OldState");

        builder.HasIndex(j => j.InitiatedBy)
            .HasDatabaseName("IX_OrderJourneys_InitiatedBy");
    }
}
