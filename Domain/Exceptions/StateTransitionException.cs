using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid order state transition is attempted
/// Follows clean architecture principles by keeping domain-specific exceptions in the Domain layer
/// </summary>
public class StateTransitionException : Exception
{
    /// <summary>
    /// The current state of the order
    /// </summary>
    public OrderState CurrentState { get; }

    /// <summary>
    /// The attempted next state
    /// </summary>
    public OrderState AttemptedState { get; }

    /// <summary>
    /// The order ID for context
    /// </summary>
    public OrderId OrderId { get; }

    /// <summary>
    /// Initializes a new instance of StateTransitionException
    /// </summary>
    /// <param name="orderId">The ID of the order</param>
    /// <param name="currentState">The current state of the order</param>
    /// <param name="attemptedState">The attempted next state</param>
    /// <param name="message">Custom error message</param>
    public StateTransitionException(
        OrderId orderId,
        OrderState currentState,
        OrderState attemptedState,
        string? message = null)
        : base(message ?? $"Invalid state transition from {currentState} to {attemptedState} for order {orderId}")
    {
        OrderId = orderId;
        CurrentState = currentState;
        AttemptedState = attemptedState;
    }

    /// <summary>
    /// Initializes a new instance of StateTransitionException with inner exception
    /// </summary>
    /// <param name="orderId">The ID of the order</param>
    /// <param name="currentState">The current state of the order</param>
    /// <param name="attemptedState">The attempted next state</param>
    /// <param name="message">Custom error message</param>
    /// <param name="innerException">The inner exception</param>
    public StateTransitionException(
        OrderId orderId,
        OrderState currentState,
        OrderState attemptedState,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        OrderId = orderId;
        CurrentState = currentState;
        AttemptedState = attemptedState;
    }

    /// <summary>
    /// Gets a detailed error message with context
    /// </summary>
    public string GetDetailedMessage()
    {
        return $"State transition failed for Order {OrderId}: " +
               $"Cannot transition from {CurrentState} to {AttemptedState}. " +
               $"Reason: {Message}";
    }

    /// <summary>
    /// Gets valid next states for the current state
    /// </summary>
    public IEnumerable<OrderState> GetValidNextStates()
    {
        return Enum.GetValues<OrderState>()
            .Where(state => OrderTransitionValidator.IsValidTransition(CurrentState, state));
    }
}
