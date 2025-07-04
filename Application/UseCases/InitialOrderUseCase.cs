using Application.DTOs;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Domain.Aggregates;
using Domain.Services;

namespace Application.UseCases;

/// <summary>
/// Use case for handling initial order creation - 5th Iteration Implementation
/// Follows clean architecture principles and implements the complete order creation flow
/// </summary>
public class InitialOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderPaymentRepository _paymentRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IOrderWorkflowService _workflowService;

    public InitialOrderUseCase(
        IOrderRepository orderRepository,
        IOrderPaymentRepository paymentRepository,
        IOrderItemRepository orderItemRepository,
        IOrderWorkflowService workflowService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
    }

    /// <summary>
    /// Executes the initial order creation use case following 5th iteration requirements
    /// Flow: 1. Create order -> 2. Create order items -> 3. Create payment -> 4. Start workflow
    /// </summary>
    /// <param name="request">The initial order request with order items</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The initial order response</returns>
    public async Task<InitialOrderResponse> ExecuteAsync(InitialOrderRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request.ReferenceId))
        {
            throw new ArgumentException("ReferenceId is required for idempotent operation", nameof(request.ReferenceId));
        }

        if (request.OrderItems == null || !request.OrderItems.Any())
        {
            throw new ArgumentException("At least one order item is required", nameof(request.OrderItems));
        }

        // Check if order with ReferenceId already exists (idempotency check)
        var existingOrder = await _orderRepository.GetByReferenceIdAsync(request.ReferenceId, cancellationToken);

        if (existingOrder != null)
        {
            // Return existing order data (idempotent behavior)
            var existingPayments = await _paymentRepository.GetByOrderIdAsync(existingOrder.Id, cancellationToken);
            var firstPayment = existingPayments.FirstOrDefault();

            if (firstPayment == null)
            {
                throw new InvalidOperationException($"Order {existingOrder.ReferenceId} exists but has no payments");
            }

            return new InitialOrderResponse(
                OrderId: existingOrder.Id.Value,
                ReferenceId: existingOrder.ReferenceId,
                PaymentId: firstPayment.Id.Value,
                PaymentStatus: firstPayment.Status.ToString(),
                Created: false
            );
        }

        // Step 1: Create order first by reference ID  
        var order = Order.Create(request.ReferenceId);
        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        // Step 2: Create order items
        var orderItems = new List<OrderItem>();
        foreach (var itemRequest in request.OrderItems)
        {
            var orderItem = OrderItem.Create(
                order.Id,
                ProductId.From(itemRequest.ProductId),
                itemRequest.Quantity,
                itemRequest.NetAmount,
                itemRequest.GrossAmount,
                itemRequest.Currency
            );

            orderItems.Add(orderItem);
            await _orderItemRepository.AddAsync(orderItem, cancellationToken);
        }
        await _orderItemRepository.SaveChangesAsync(cancellationToken);

        // Step 3: Create order payment with payment method and set status to pending
        var orderAggregate = OrderAggregate.FromExistingOrder(order);

        // Calculate total payment amount from order items
        var totalAmount = orderItems.Sum(item => item.GrossAmount * item.Quantity);
        var currency = orderItems.FirstOrDefault()?.Currency ?? "THB";

        // Use domain factory method for payment method creation
        var paymentMethod = PaymentMethod.FromString(request.PaymentMethod);
        var paymentId = orderAggregate.ProcessPayment(paymentMethod, totalAmount, currency);

        // Get the payment entity from the aggregate
        var payment = orderAggregate.Payments.First(p => p.Id == paymentId);

        // Save payment to repository
        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        // Step 4: Create temporal workflow
        var workflowId = await _workflowService.StartOrderProcessingWorkflowAsync(order.Id.Value, cancellationToken);

        // Update order with workflow ID
        // order.SetWorkflowId(workflowId);
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return new InitialOrderResponse(
            OrderId: order.Id.Value,
            ReferenceId: order.ReferenceId,
            PaymentId: payment.Id.Value,
            PaymentStatus: payment.Status.ToString(),
            Created: true
        );
    }
}
