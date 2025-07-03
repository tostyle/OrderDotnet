using Application.DTOs;
using Application.Services;
using Domain.Services;
using Domain.ValueObjects;
using Domain.Repositories;

namespace Application.UseCases;

/// <summary>
/// Use case for canceling orders and sending workflow signals
/// Follows clean architecture principles by orchestrating domain services
/// </summary>
public class CancelOrderUseCase
{
    private readonly OrderService _orderService;
    private readonly IOrderWorkflowService _workflowService;
    private readonly IOrderRepository _orderRepository;

    public CancelOrderUseCase(
        OrderService orderService,
        IOrderWorkflowService workflowService,
        IOrderRepository orderRepository)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    /// <summary>
    /// Executes the order cancellation use case
    /// Flow: 1. Validate order exists -> 2. Check workflow exists -> 3. Send cancel signal to workflow
    /// </summary>
    /// <param name="request">The order cancellation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The order cancellation response</returns>
    public async Task<CancelOrderUseCaseResponse> ExecuteAsync(CancelOrderUseCaseRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request
        if (request.OrderId == Guid.Empty)
        {
            throw new ArgumentException("OrderId is required", nameof(request.OrderId));
        }

        var orderId = OrderId.From(request.OrderId);

        // Get order to ensure it exists
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {request.OrderId} not found");
        }

        // Check if order has associated workflow
        if (string.IsNullOrEmpty(order.WorkflowId))
        {
            throw new InvalidOperationException($"Order {request.OrderId} does not have an associated workflow");
        }

        // Check if order can be cancelled (business rule - can extend this with more sophisticated logic)
        if (order.OrderState.ToString() == "DELIVERED" || order.OrderState.ToString() == "CANCELED")
        {
            throw new InvalidOperationException($"Order {request.OrderId} cannot be cancelled as it is already {order.OrderState}");
        }

        // Send cancel order signal to workflow
        await _workflowService.SendCancelOrderSignalAsync(
            orderId: request.OrderId,
            reason: request.Reason ?? "Manual cancellation via API",
            cancellationToken: cancellationToken
        );

        return new CancelOrderUseCaseResponse(
            OrderId: request.OrderId,
            WorkflowId: order.WorkflowId,
            Status: "Cancel signal sent",
            Reason: request.Reason ?? "Manual cancellation via API",
            CancelledAt: DateTime.UtcNow
        );
    }
}

/// <summary>
/// Request DTO for CancelOrderUseCase
/// </summary>
public record CancelOrderUseCaseRequest(
    Guid OrderId,
    string? Reason = null
);

/// <summary>
/// Response DTO for CancelOrderUseCase
/// </summary>
public record CancelOrderUseCaseResponse(
    Guid OrderId,
    string? WorkflowId,
    string Status,
    string Reason,
    DateTime CancelledAt
);
