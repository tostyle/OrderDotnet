using Application.DTOs;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Domain.Services;
using Domain.Aggregates;

namespace Application.UseCases;

/// <summary>
/// Use case for changing order status to Pending
/// Implements workflow reset functionality to transition order back to pending state
/// </summary>
public class ChangeOrderStatusToPendingUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderJourneyRepository _orderJourneyRepository;
    private readonly IOrderLogRepository _orderLogRepository;
    private readonly IWorkflowService _workflowService;

    public ChangeOrderStatusToPendingUseCase(
        IOrderRepository orderRepository,
        IOrderJourneyRepository orderJourneyRepository,
        IOrderLogRepository orderLogRepository,
        IWorkflowService workflowService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _orderJourneyRepository = orderJourneyRepository ?? throw new ArgumentNullException(nameof(orderJourneyRepository));
        _orderLogRepository = orderLogRepository ?? throw new ArgumentNullException(nameof(orderLogRepository));
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
    }

    /// <summary>
    /// Changes order status to Pending and resets workflow to TransitionToPendingState activity
    /// </summary>
    /// <param name="request">Request containing orderId</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing updated order information</returns>
    public async Task<ChangeOrderStatusToPendingResponse> ExecuteAsync(
        ChangeOrderStatusToPendingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Console.WriteLine($"[INFO] Starting change order status to pending for OrderId: {request.OrderId}");

        // Convert Guid to OrderId value object
        var orderIdValue = OrderId.From(request.OrderId);

        // Retrieve the order from repository
        var order = await _orderRepository.GetByIdAsync(orderIdValue, cancellationToken);
        if (order == null)
        {
            Console.WriteLine($"[WARNING] Order with ID {request.OrderId} not found");
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");
        }

        var previousState = order.OrderState;

        // Check if order is already in Pending state
        if (order.OrderState == OrderState.Pending)
        {
            Console.WriteLine($"[INFO] Order {request.OrderId} is already in Pending state");
            return new ChangeOrderStatusToPendingResponse
            {
                OrderId = request.OrderId,
                PreviousState = previousState,
                NewState = OrderState.Pending,
                IsAlreadyPending = true,
                Message = "Order is already in Pending state"
            };
        }

        // Business Rule: Only Cancelled orders can be changed to Pending status
        if (order.OrderState != OrderState.Cancelled)
        {
            Console.WriteLine($"[WARNING] Order {request.OrderId} cannot be changed to Pending. Current state: {order.OrderState}. Only Cancelled orders can be reset to Pending.");
            throw new InvalidOperationException($"Only Cancelled orders can be changed to Pending status. Current order state is {order.OrderState}");
        }

        // Create order aggregate to handle the state transition
        var orderAggregate = OrderAggregate.FromExistingOrder(order);

        // Execute the state transition to Pending
        orderAggregate.TransitionOrderState(OrderState.Pending, request.Reason);

        // Save the updated order to the repository
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        // Create OrderJourney record for audit trail
        var orderJourney = OrderJourney.Create(
            orderId: orderIdValue,
            oldState: previousState,
            newState: OrderState.Pending,
            reason: request.Reason,
            initiatedBy: request.InitiatedBy ?? "System",
            metadata: $"{{\"transitionedAt\":\"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}\",\"version\":{order.Version},\"resetType\":\"StatusToPending\"}}"
        );

        await _orderJourneyRepository.AddAsync(orderJourney);

        // Create OrderLog record for logging
        var orderLog = OrderLog.CreateStateTransition(
            orderId: orderIdValue,
            oldState: previousState,
            newState: OrderState.Pending,
            reason: request.Reason,
            performedBy: request.InitiatedBy ?? "System",
            source: "ChangeOrderStatusToPendingUseCase"
        );

        await _orderLogRepository.AddAsync(orderLog);

        // Save changes for audit trail
        await _orderJourneyRepository.SaveChangesAsync(cancellationToken);
        await _orderLogRepository.SaveChangesAsync(cancellationToken);

        // Reset workflow to TransitionToPendingState activity
        try
        {
            await _workflowService.ResetWorkflowToPendingStateAsync(request.OrderId, cancellationToken);
            Console.WriteLine($"[INFO] Successfully reset workflow for order {request.OrderId} to TransitionToPendingState activity");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to reset workflow for order {request.OrderId}: {ex.Message}");
            // Continue execution - workflow reset failure should not prevent order state change
        }

        Console.WriteLine($"[INFO] Successfully changed Order {request.OrderId} from {previousState} to Pending. Reason: {request.Reason ?? "No reason provided"}");

        // Return the response with updated order information
        return new ChangeOrderStatusToPendingResponse
        {
            OrderId = request.OrderId,
            PreviousState = previousState,
            NewState = OrderState.Pending,
            IsAlreadyPending = false,
            Message = "Order status successfully changed to Pending"
        };
    }
}
