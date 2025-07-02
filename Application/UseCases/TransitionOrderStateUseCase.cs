using Application.DTOs;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Domain.Aggregates;
using Domain.Exceptions;

namespace Application.UseCases;

/// <summary>
/// Use case for handling order state transitions
/// Follows clean architecture principles and implements secure state transition management
/// Creates audit trail records for state transitions and logs
/// </summary>
public class TransitionOrderStateUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderJourneyRepository _orderJourneyRepository;
    private readonly IOrderLogRepository _orderLogRepository;

    public TransitionOrderStateUseCase(
        IOrderRepository orderRepository,
        IOrderJourneyRepository orderJourneyRepository,
        IOrderLogRepository orderLogRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _orderJourneyRepository = orderJourneyRepository ?? throw new ArgumentNullException(nameof(orderJourneyRepository));
        _orderLogRepository = orderLogRepository ?? throw new ArgumentNullException(nameof(orderLogRepository));
    }

    /// <summary>
    /// Transitions an order to a new state
    /// </summary>
    /// <param name="orderId">The unique identifier of the order</param>
    /// <param name="orderState">The target order state</param>
    /// <param name="reason">Optional reason for the state transition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing the updated order information</returns>
    public async Task<TransitionOrderStateResponse> TransitionOrderState(
        Guid orderId,
        OrderState orderState,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {

        Console.WriteLine($"[INFO] Starting order state transition for OrderId: {orderId} to State: {orderState}");

        // Convert Guid to OrderId value object
        var orderIdValue = OrderId.From(orderId);

        // Retrieve the order from repository
        var order = await _orderRepository.GetByIdAsync(orderIdValue, cancellationToken);
        if (order == null)
        {
            Console.WriteLine($"[WARNING] Order with ID {orderId} not found");
            throw new InvalidOperationException($"Order with ID {orderId} not found");
        }

        // Create order aggregate to handle the state transition
        var orderAggregate = OrderAggregate.FromExistingOrder(order);
        var previousState = order.OrderState;

        // Execute the state transition using the aggregate
        orderAggregate.TransitionOrderState(orderState, reason);

        // Save the updated order to the repository
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        // Create OrderJourney record for audit trail
        var orderJourney = OrderJourney.Create(
            orderId: orderIdValue,
            oldState: previousState,
            newState: orderState,
            reason: reason,
            initiatedBy: "System", // TODO: Get from current user context
            metadata: $"{{\"transitionedAt\":\"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}\",\"version\":{order.Version}}}"
        );

        await _orderJourneyRepository.AddAsync(orderJourney);

        // Create OrderLog record for logging
        var orderLog = OrderLog.CreateStateTransition(
            orderId: orderIdValue,
            oldState: previousState,
            newState: orderState,
            reason: reason,
            performedBy: "System", // TODO: Get from current user context
            source: "TransitionOrderStateUseCase"
        );

        await _orderLogRepository.AddAsync(orderLog);

        // Save changes for audit trail
        await _orderJourneyRepository.SaveChangesAsync();
        await _orderLogRepository.SaveChangesAsync();

        Console.WriteLine($"[INFO] Successfully transitioned Order {orderId} from {previousState} to {orderState}. Reason: {reason ?? "No reason provided"}");

        // Return the response with updated order information
        return new TransitionOrderStateResponse(
            OrderId: orderId,
            PreviousState: previousState.ToString(),
            NewState: orderState.ToString(),
            Success: true,
            Message: $"Order successfully transitioned from {previousState} to {orderState}",
            Reason: reason,
            TransitionedAt: DateTime.UtcNow,
            OrderVersion: order.Version
        );
    }



    public async Task<TransitionOrderStateResponse> TransitionToPendingState(
        Guid orderId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        // Call the main transition method with Pending state
        return await TransitionOrderState(orderId, OrderState.Pending, reason, cancellationToken);
    }

    public async Task<TransitionOrderStateResponse> TransitionToPaidState(
        Guid orderId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        // Call the main transition method with Completed state
        return await TransitionOrderState(orderId, OrderState.Paid, reason, cancellationToken);
    }

    public async Task<TransitionOrderStateResponse> TransitionToCompletedState(
        Guid orderId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        // Call the main transition method with Completed state
        return await TransitionOrderState(orderId, OrderState.Completed, reason, cancellationToken);
    }

    public async Task<TransitionOrderStateResponse> TransitionToCancelledState(
        Guid orderId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        // Call the main transition method with Cancelled state
        return await TransitionOrderState(orderId, OrderState.Cancelled, reason, cancellationToken);
    }
}