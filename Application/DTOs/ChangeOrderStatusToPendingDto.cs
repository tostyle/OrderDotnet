using Domain.Entities;

namespace Application.DTOs;

/// <summary>
/// Request DTO for changing order status to pending
/// </summary>
public class ChangeOrderStatusToPendingRequest
{
    /// <summary>
    /// The unique identifier of the order to change status
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Optional reason for changing status to pending
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Who initiated this status change (defaults to "System")
    /// </summary>
    public string? InitiatedBy { get; set; }
}

/// <summary>
/// Response DTO for change order status to pending operation
/// </summary>
public class ChangeOrderStatusToPendingResponse
{
    /// <summary>
    /// The unique identifier of the order
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// The previous order state before the change
    /// </summary>
    public OrderState PreviousState { get; set; }

    /// <summary>
    /// The new order state (should be Pending)
    /// </summary>
    public OrderState NewState { get; set; }

    /// <summary>
    /// Indicates if the order was already in Pending state (idempotent operation)
    /// </summary>
    public bool IsAlreadyPending { get; set; }

    /// <summary>
    /// Descriptive message about the operation result
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for updating order state via the generic state endpoint
/// </summary>
public class UpdateOrderStateRequest
{
    /// <summary>
    /// The unique identifier of the order to update
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Optional reason for the state change
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Who initiated this state change
    /// </summary>
    public string? InitiatedBy { get; set; }
}
