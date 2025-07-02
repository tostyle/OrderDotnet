using Domain.Entities;
using Domain.Exceptions;

namespace Domain.Aggregates;

/// <summary>
/// Partial class for OrderAggregate handling state transitions
/// Implements clean architecture principles with proper error handling and logging
/// </summary>
public partial class OrderAggregate
{
    /// <summary>
    /// Transitions the order to the specified next state with validation
    /// Implements idempotent operations and proper error handling
    /// </summary>
    /// <param name="nextState">The desired next state for the order</param>
    /// <param name="reason">Optional reason for the state transition</param>
    /// <exception cref="ArgumentException">Thrown when nextState is invalid</exception>
    /// <exception cref="StateTransitionException">Thrown when the state transition is not allowed</exception>
    public void TransitionOrderState(OrderState nextState, string? reason = null)
    {
        // Validate input parameter
        if (!Enum.IsDefined(typeof(OrderState), nextState))
        {
            throw new ArgumentException($"Invalid order state: {nextState}", nameof(nextState));
        }

        var currentState = _order.OrderState;

        // Check if current state equals next state - idempotent operation
        if (currentState == nextState)
        {
            // No transition needed - idempotent behavior
            return;
        }

        // Check with CanTransitionTo function for validation
        if (!CanTransitionTo(nextState))
        {
            // Create and throw StateTransitionException with detailed context
            var exception = new StateTransitionException(
                orderId: _order.Id,
                currentState: currentState,
                attemptedState: nextState,
                message: CreateTransitionErrorMessage(currentState, nextState, reason));

            throw exception;
        }

        // Valid transition - update order status
        var previousState = _order.OrderState;
        _order.OrderState = nextState;
        _order.UpdatedAt = DateTime.UtcNow;
        _order.Version++;

        // Log successful transition for audit trail
        LogStateTransition(previousState, nextState, reason);

        // TODO: Raise domain events for state transition
        // RaiseDomainEvent(new OrderStateTransitionedEvent(_order.Id, previousState, nextState, reason, DateTime.UtcNow));
    }

    /// <summary>
    /// Creates a detailed error message for failed state transitions
    /// </summary>
    /// <param name="currentState">The current order state</param>
    /// <param name="attemptedState">The attempted next state</param>
    /// <param name="reason">Optional reason for the transition attempt</param>
    /// <returns>Formatted error message with context</returns>
    private string CreateTransitionErrorMessage(OrderState currentState, OrderState attemptedState, string? reason)
    {
        var baseMessage = $"Invalid state transition from {currentState} to {attemptedState}";

        if (!string.IsNullOrWhiteSpace(reason))
        {
            baseMessage += $". Reason: {reason}";
        }

        var validStates = GetValidNextStates();
        if (validStates.Any())
        {
            baseMessage += $". Valid next states: {string.Join(", ", validStates)}";
        }
        else
        {
            baseMessage += ". No valid transitions available from current state";
        }

        return baseMessage;
    }

    /// <summary>
    /// Logs state transition for audit and debugging purposes
    /// </summary>
    /// <param name="previousState">The previous order state</param>
    /// <param name="newState">The new order state</param>
    /// <param name="reason">Optional reason for the transition</param>
    private void LogStateTransition(OrderState previousState, OrderState newState, string? reason)
    {
        // In a real implementation, this would use proper logging infrastructure
        // For now, we'll use a simple approach that can be enhanced later
        var logMessage = $"Order {_order.Id}: State transition from {previousState} to {newState}";

        if (!string.IsNullOrWhiteSpace(reason))
        {
            logMessage += $" - Reason: {reason}";
        }

        logMessage += $" at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        // TODO: Replace with proper logging when ILogger is available
        Console.WriteLine($"[StateTransition] {logMessage}");
    }

    /// <summary>
    /// Extension method to validate if a state transition is safe for business operations
    /// Implements additional business rule validation beyond basic state machine rules
    /// </summary>
    /// <param name="nextState">The desired next state</param>
    /// <returns>True if the transition passes all business rule validations</returns>
    public bool IsBusinessRuleCompliantTransition(OrderState nextState)
    {
        // Basic validation first
        if (!CanTransitionTo(nextState))
        {
            return false;
        }

        // Additional business rule validations
        return nextState switch
        {
            OrderState.Paid => ValidateCanTransitionToPaid(),
            OrderState.Completed => ValidateCanTransitionToCompleted(),
            OrderState.Cancelled => ValidateCanTransitionToCancelled(),
            OrderState.Refunded => ValidateCanTransitionToRefunded(),
            _ => true // Other transitions only need basic validation
        };
    }

    /// <summary>
    /// Calculates the total amount for the order based on order items
    /// </summary>
    /// <returns>Total gross amount of all order items</returns>
    private decimal CalculateOrderTotalAmount()
    {
        return _order.OrderItems?.Sum(item => item.CalculateTotalGrossAmount()) ?? 0m;
    }

    /// <summary>
    /// Validates business rules for transitioning to Paid state
    /// </summary>
    private bool ValidateCanTransitionToPaid()
    {
        // Order must have sufficient payments
        var orderTotalAmount = CalculateOrderTotalAmount();

        // If order has no items (total amount is 0), we still require at least some payment
        // This prevents transitioning to Paid state without any actual payment processing
        if (orderTotalAmount == 0)
        {
            return TotalPaidAmount > 0; // Require some payment even for zero-amount orders
        }

        return TotalPaidAmount >= orderTotalAmount;
    }

    /// <summary>
    /// Validates business rules for transitioning to Completed state
    /// </summary>
    private bool ValidateCanTransitionToCompleted()
    {
        // Order must be fully paid and have stock confirmed
        var orderTotalAmount = CalculateOrderTotalAmount();
        return IsFullyPaid(orderTotalAmount) && HasStockReserved();
    }

    /// <summary>
    /// Validates business rules for transitioning to Cancelled state
    /// </summary>
    private bool ValidateCanTransitionToCancelled()
    {
        // Orders can generally be cancelled unless they're already completed
        return _order.OrderState != OrderState.Completed;
    }

    /// <summary>
    /// Validates business rules for transitioning to Refunded state
    /// </summary>
    private bool ValidateCanTransitionToRefunded()
    {
        // Can only refund orders that have been paid
        return _order.OrderState == OrderState.Paid || _order.OrderState == OrderState.Completed;
    }

    /// <summary>
    /// Provides a safe transition method that includes business rule validation
    /// Implements the full business logic for state transitions
    /// </summary>
    /// <param name="nextState">The desired next state</param>
    /// <param name="reason">Optional reason for the transition</param>
    /// <param name="enforceBusinessRules">Whether to enforce additional business rules beyond state machine validation</param>
    /// <exception cref="StateTransitionException">Thrown when transition validation fails</exception>
    public void SafeTransitionOrderState(OrderState nextState, string? reason = null, bool enforceBusinessRules = true)
    {
        if (enforceBusinessRules && !IsBusinessRuleCompliantTransition(nextState))
        {
            var errorMessage = CreateBusinessRuleErrorMessage(nextState, reason);
            throw new StateTransitionException(_order.Id, _order.OrderState, nextState, errorMessage);
        }

        // Perform the actual transition
        TransitionOrderState(nextState, reason);
    }

    /// <summary>
    /// Creates error message for business rule violations
    /// </summary>
    private string CreateBusinessRuleErrorMessage(OrderState attemptedState, string? reason)
    {
        var message = $"Business rule validation failed for transition to {attemptedState}";

        if (!string.IsNullOrWhiteSpace(reason))
        {
            message += $". Reason: {reason}";
        }

        // Add specific business rule context
        message += attemptedState switch
        {
            OrderState.Paid => ". Order payments insufficient or invalid",
            OrderState.Completed => ". Order not fully paid or stock not confirmed",
            OrderState.Refunded => ". Order must be paid or completed before refunding",
            _ => ". General business rule violation"
        };

        return message;
    }
}