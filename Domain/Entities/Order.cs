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
    public string? WorkflowId { get; set; }

    public string ReferenceId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property for related loyalty transactions
    /// </summary>
    public virtual ICollection<OrderLoyalty> LoyaltyTransactions { get; set; } = new List<OrderLoyalty>();

    /// <summary>
    /// Navigation property for related stock reservations
    /// </summary>
    public virtual ICollection<OrderStock> StockReservations { get; set; } = new List<OrderStock>();

    /// <summary>
    /// Navigation property for related payment
    /// </summary>
    public virtual OrderPayment? Payment { get; set; }

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
    public static Order Create(string? referenceId = null)
    {
        var orderId = OrderId.New();
        var now = DateTime.UtcNow;

        var order = new Order
        {
            Id = orderId,
            OrderState = OrderState.Initial,
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1,
            ReferenceId = referenceId ?? Guid.NewGuid().ToString()
        };

        return order;
    }

    /// <summary>
    /// Sets the workflow ID for temporal tracking
    /// </summary>
    public void SetWorkflowId(string workflowId)
    {
        WorkflowId = workflowId ?? throw new ArgumentNullException(nameof(workflowId));
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }


}
