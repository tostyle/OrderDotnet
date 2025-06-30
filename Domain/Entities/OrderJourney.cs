using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Entity to record order state transitions for audit trail and tracking
/// Contains old state, new state, and transition date information
/// </summary>
public class OrderJourney
{
    /// <summary>
    /// Unique identifier for the order journey record
    /// </summary>
    public OrderJourneyId Id { get; private set; }

    /// <summary>
    /// The order this journey record belongs to
    /// </summary>
    public OrderId OrderId { get; private set; }

    /// <summary>
    /// The previous state before the transition
    /// </summary>
    public OrderState OldState { get; private set; }

    /// <summary>
    /// The new state after the transition
    /// </summary>
    public OrderState NewState { get; private set; }

    /// <summary>
    /// When the state transition occurred
    /// </summary>
    public DateTime TransitionDate { get; private set; }

    /// <summary>
    /// Optional reason for the state transition
    /// </summary>
    public string? Reason { get; private set; }

    /// <summary>
    /// Optional user or system that initiated the transition
    /// </summary>
    public string? InitiatedBy { get; private set; }

    /// <summary>
    /// Additional metadata about the transition (JSON format)
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// When this journey record was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Navigation property back to the parent Order
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    // Private constructor for EF Core
    private OrderJourney()
    {
        Id = null!;
        OrderId = null!;
    }

    /// <summary>
    /// Creates a new order journey record for state transition tracking
    /// </summary>
    /// <param name="orderId">The order this journey belongs to</param>
    /// <param name="oldState">The previous state</param>
    /// <param name="newState">The new state</param>
    /// <param name="reason">Optional reason for the transition</param>
    /// <param name="initiatedBy">Optional user or system that initiated the transition</param>
    /// <param name="metadata">Optional additional metadata</param>
    /// <returns>New OrderJourney instance</returns>
    public static OrderJourney Create(
        OrderId orderId,
        OrderState oldState,
        OrderState newState,
        string? reason = null,
        string? initiatedBy = null,
        string? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        if (!Enum.IsDefined(typeof(OrderState), oldState))
            throw new ArgumentException($"Invalid old state: {oldState}", nameof(oldState));

        if (!Enum.IsDefined(typeof(OrderState), newState))
            throw new ArgumentException($"Invalid new state: {newState}", nameof(newState));

        var now = DateTime.UtcNow;

        return new OrderJourney
        {
            Id = OrderJourneyId.New(),
            OrderId = orderId,
            OldState = oldState,
            NewState = newState,
            TransitionDate = now,
            Reason = reason?.Trim(),
            InitiatedBy = initiatedBy?.Trim(),
            Metadata = metadata?.Trim(),
            CreatedAt = now
        };
    }

    /// <summary>
    /// Creates a journey record for initial order creation
    /// </summary>
    /// <param name="orderId">The order this journey belongs to</param>
    /// <param name="initialState">The initial state of the order</param>
    /// <param name="initiatedBy">Who created the order</param>
    /// <returns>New OrderJourney instance for order creation</returns>
    public static OrderJourney CreateInitial(
        OrderId orderId,
        OrderState initialState,
        string? initiatedBy = null)
    {
        return Create(
            orderId: orderId,
            oldState: initialState, // For initial creation, old and new are the same
            newState: initialState,
            reason: "Order created",
            initiatedBy: initiatedBy,
            metadata: "{\"type\":\"order_creation\"}"
        );
    }

    /// <summary>
    /// Updates the metadata for this journey record
    /// </summary>
    /// <param name="metadata">New metadata in JSON format</param>
    public void UpdateMetadata(string? metadata)
    {
        Metadata = metadata?.Trim();
    }

    /// <summary>
    /// Checks if this journey represents a state transition (vs initial creation)
    /// </summary>
    /// <returns>True if old state differs from new state</returns>
    public bool IsStateTransition()
    {
        return OldState != NewState;
    }

    /// <summary>
    /// Gets a description of the transition
    /// </summary>
    /// <returns>Human-readable description of the state change</returns>
    public string GetTransitionDescription()
    {
        if (!IsStateTransition())
        {
            return $"Order initialized in {NewState} state";
        }

        var description = $"State changed from {OldState} to {NewState}";

        if (!string.IsNullOrWhiteSpace(Reason))
        {
            description += $" - {Reason}";
        }

        return description;
    }

    /// <summary>
    /// Gets the duration since this transition occurred
    /// </summary>
    /// <returns>TimeSpan since the transition</returns>
    public TimeSpan GetTimeSinceTransition()
    {
        return DateTime.UtcNow - TransitionDate;
    }
}
