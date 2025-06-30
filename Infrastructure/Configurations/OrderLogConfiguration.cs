using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Configurations;

/// <summary>
/// Entity configuration for OrderLog entity
/// Configures the comprehensive order activity audit trail
/// </summary>
public class OrderLogConfiguration : IEntityTypeConfiguration<OrderLog>
{
    public void Configure(EntityTypeBuilder<OrderLog> builder)
    {
        builder.ToTable("OrderLogs");

        // Primary key
        builder.HasKey(l => l.Id);

        // Configure OrderLogId value object
        builder.Property(l => l.Id)
            .HasConversion(
                logId => logId.Value,
                value => OrderLogId.From(value))
            .IsRequired();

        // Configure OrderId value object
        builder.Property(l => l.OrderId)
            .HasConversion(
                orderId => orderId.Value,
                value => OrderId.From(value))
            .IsRequired();

        // Configure required fields
        builder.Property(l => l.ActionType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.Description)
            .HasMaxLength(2000)
            .IsRequired();

        // Configure LogLevel enum
        builder.Property(l => l.Level)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Configure dates
        builder.Property(l => l.ActionDate)
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        // Configure optional fields
        builder.Property(l => l.PerformedBy)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(l => l.Source)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(l => l.Data)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(l => l.ErrorMessage)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(l => l.StackTrace)
            .HasColumnType("text")
            .IsRequired(false);

        // Performance indexes for common query patterns
        builder.HasIndex(l => l.OrderId)
            .HasDatabaseName("IX_OrderLogs_OrderId");

        builder.HasIndex(l => l.ActionType)
            .HasDatabaseName("IX_OrderLogs_ActionType");

        builder.HasIndex(l => l.Level)
            .HasDatabaseName("IX_OrderLogs_Level");

        builder.HasIndex(l => l.ActionDate)
            .HasDatabaseName("IX_OrderLogs_ActionDate");

        builder.HasIndex(l => l.PerformedBy)
            .HasDatabaseName("IX_OrderLogs_PerformedBy");

        // Compound indexes for common filtering scenarios
        builder.HasIndex(l => new { l.OrderId, l.ActionType })
            .HasDatabaseName("IX_OrderLogs_OrderId_ActionType");

        builder.HasIndex(l => new { l.OrderId, l.Level })
            .HasDatabaseName("IX_OrderLogs_OrderId_Level");

        builder.HasIndex(l => new { l.OrderId, l.ActionDate })
            .HasDatabaseName("IX_OrderLogs_OrderId_ActionDate");

        // Index for error logs specifically
        builder.HasIndex(l => new { l.Level, l.ActionDate })
            .HasFilter("\"Level\" = 'Error'")
            .HasDatabaseName("IX_OrderLogs_Errors_ActionDate");
    }
}
