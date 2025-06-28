using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Order aggregate root - represents an order in the system
/// </summary>
public class Order
{
    /// <summary>
    /// Unique identifier for the order
    /// </summary>
    public OrderId Id { get; private set; }

    /// <summary>
    /// Current state of the order
    /// </summary>
    public OrderState OrderState { get; internal set; }

    /// <summary>
    /// When the order was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the order was last updated
    /// </summary>
    public DateTime UpdatedAt { get; internal set; }

    /// <summary>
    /// Version for optimistic concurrency control
    /// </summary>
    public long Version { get; internal set; }

    /// <summary>
    /// Temporal workflow ID for tracking workflow execution
    /// </summary>
    public string? WorkflowId { get; private set; }

    public string ReferenceId { get; private set; } = string.Empty;

    /// <summary>
    /// Domain events that have occurred
    /// </summary>

    // Private constructor for EF Core
    private Order()
    {
        Id = null!;
    }

    /// <summary>
    /// Creates a new order in PENDING state
    /// </summary>
    public static Order Create()
    {
        var orderId = OrderId.New();
        var now = DateTime.UtcNow;

        var order = new Order
        {
            Id = orderId,
            OrderState = OrderState.Initial,
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1
        };


        return order;
    }


}
