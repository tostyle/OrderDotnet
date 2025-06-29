using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Domain.Aggregates;
using Application.DTOs;

namespace Application.Services;



/// <summary>
/// Implementation of OrderService for 3rd iteration requirements
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderPaymentRepository _paymentRepository;
    private readonly IOrderLoyaltyRepository _loyaltyRepository;
    private readonly IOrderStockRepository _stockRepository;
    private readonly IOrderItemRepository _orderItemRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderPaymentRepository paymentRepository,
        IOrderLoyaltyRepository loyaltyRepository,
        IOrderStockRepository stockRepository,
        IOrderItemRepository orderItemRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _loyaltyRepository = loyaltyRepository ?? throw new ArgumentNullException(nameof(loyaltyRepository));
        _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
        _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
    }

    /// <summary>
    /// 3. EarnLoyalty - make method calculate and save to repo
    /// </summary>
    public async Task<LoyaltyTransactionResponse> EarnLoyaltyAsync(EarnLoyaltyRequest request, CancellationToken cancellationToken = default)
    {
        // Find order by ID
        var orderId = OrderId.From(request.OrderId);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {request.OrderId} not found");
        }

        // Create OrderAggregate
        var orderAggregate = OrderAggregate.FromExistingOrder(order);

        // Earn loyalty points
        var transactionId = orderAggregate.EarnLoyaltyPoints(request.Points, request.Description);

        // Get the loyalty transaction from the aggregate
        var loyaltyTransaction = orderAggregate.LoyaltyTransactions.First(l => l.Id == transactionId);

        // Save loyalty transaction to repository
        await _loyaltyRepository.AddAsync(loyaltyTransaction, cancellationToken);
        await _loyaltyRepository.SaveChangesAsync(cancellationToken);

        return new LoyaltyTransactionResponse(
            TransactionId: loyaltyTransaction.Id.Value,
            TransactionType: loyaltyTransaction.TransactionType.ToString(),
            Points: loyaltyTransaction.Points,
            Description: loyaltyTransaction.Description,
            TransactionDate: loyaltyTransaction.TransactionDate
        );
    }

    /// <summary>
    /// 3. BurnLoyalty - make method calculate and save to repo
    /// </summary>
    public async Task<LoyaltyTransactionResponse> BurnLoyaltyAsync(BurnLoyaltyRequest request, CancellationToken cancellationToken = default)
    {
        // Find order by ID
        var orderId = OrderId.From(request.OrderId);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {request.OrderId} not found");
        }

        // Create OrderAggregate
        var orderAggregate = OrderAggregate.FromExistingOrder(order);

        // Burn loyalty points
        var transactionId = orderAggregate.BurnLoyaltyPoints(request.Points, request.Description);

        // Get the loyalty transaction from the aggregate
        var loyaltyTransaction = orderAggregate.LoyaltyTransactions.First(l => l.Id == transactionId);

        // Save loyalty transaction to repository
        await _loyaltyRepository.AddAsync(loyaltyTransaction, cancellationToken);
        await _loyaltyRepository.SaveChangesAsync(cancellationToken);

        return new LoyaltyTransactionResponse(
            TransactionId: loyaltyTransaction.Id.Value,
            TransactionType: loyaltyTransaction.TransactionType.ToString(),
            Points: loyaltyTransaction.Points,
            Description: loyaltyTransaction.Description,
            TransactionDate: loyaltyTransaction.TransactionDate
        );
    }

    /// <summary>
    /// 4. ProcessPayment - update OrderPayment to Status Completed
    /// </summary>
    public async Task<ProcessPaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request, CancellationToken cancellationToken = default)
    {
        // Find payment by ID
        var paymentId = PaymentId.From(request.PaymentId);
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            throw new InvalidOperationException($"Payment {request.PaymentId} not found");
        }

        // Find the order associated with this payment
        var order = await _orderRepository.GetByIdAsync(payment.OrderId, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {payment.OrderId} not found");
        }

        // Create OrderAggregate
        var orderAggregate = OrderAggregate.FromExistingOrder(order);

        // Confirm the payment using the aggregate
        orderAggregate.ConfirmPayment(paymentId);

        // Update payment status to successful (completed)
        payment.MarkAsSuccessful(request.TransactionReference, request.Notes);

        // Update repositories
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        // If order state changed, update order too
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return new ProcessPaymentResponse(
            PaymentId: payment.Id.Value,
            Status: payment.Status.ToString(),
            PaidDate: payment.PaidDate,
            TransactionReference: payment.TransactionReference
        );
    }

    public async Task<OrderDetailResponse> GetOrderDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // First, get the order to ensure it exists
        var order = await _orderRepository.GetByIdAsync(OrderId.From(orderId), cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        // Execute all related data queries in parallel for better performance
        var loyaltyTransactions = await _loyaltyRepository.GetByOrderIdAsync(order.Id, cancellationToken);
        var payments = await _paymentRepository.GetByOrderIdAsync(order.Id, cancellationToken);
        var stockReservations = await _stockRepository.GetByOrderIdAsync(order.Id, cancellationToken);



        return OrderDetailResponse.FromDomainEntities(
            order: order,
            loyaltyTransactions: loyaltyTransactions,
            payments: payments,
            stockReservations: stockReservations
        );
    }


    /// <summary>
    /// 5. StartWorkflow - associates a workflow ID with an order for tracking
    /// </summary>
    /// <param name="orderId">The order ID to start workflow for</param>
    /// <param name="workflowId">The workflow ID to associate with the order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response indicating success/failure</returns>
    public async Task<StartWorkflowResponse> StartWorkflowAsync(Guid orderId, string workflowId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(workflowId))
        {
            throw new ArgumentException("WorkflowId cannot be null or empty", nameof(workflowId));
        }

        // Find order by ID
        var order = await _orderRepository.GetByIdAsync(OrderId.From(orderId), cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        // Check if order already has a workflow associated
        if (!string.IsNullOrEmpty(order.WorkflowId))
        {
            throw new InvalidOperationException($"Order {orderId} already has workflow {order.WorkflowId} associated");
            // return new StartWorkflowResponse(
            //     OrderId: order.Id.Value,
            //     WorkflowId: order.WorkflowId
            // );
        }

        // Update the order with the workflow ID
        order.SetWorkflowId(workflowId);

        // Save changes to repository
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return new StartWorkflowResponse(
            OrderId: orderId,
            WorkflowId: workflowId
        );
    }

    /// <summary>
    /// 6. GetWorkflowStatus - gets the workflow status for an order
    /// </summary>
    /// <param name="orderId">The order ID to check workflow status for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response with workflow status information</returns>
    public async Task<WorkflowStatusResponse> GetWorkflowStatusAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // Find order by ID
        var order = await _orderRepository.GetByIdAsync(OrderId.From(orderId), cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        return new WorkflowStatusResponse(
            OrderId: orderId,
            WorkflowId: order.WorkflowId,
            HasWorkflow: !string.IsNullOrEmpty(order.WorkflowId),
            OrderState: order.OrderState.ToString(),
            LastUpdated: order.UpdatedAt
        );
    }

    /// <summary>
    /// GetOrderWithDetails - retrieve order with all related entities using repository method
    /// with navigation properties for efficient data loading
    /// </summary>
    public async Task<DetailedOrderDto?> GetOrderWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // Get order with all related entities using repository method with navigation properties
        var order = await _orderRepository.GetByIdWithDetailsAsync(OrderId.From(orderId), cancellationToken);

        if (order == null)
        {
            return null;
        }

        // Use the original FromOrder method since navigation properties are loaded
        return DetailedOrderDto.FromOrder(order);
    }

    /// <summary>
    /// GetOrderWithDetailsByReferenceId - retrieve order with all related entities by reference ID
    /// using repository method with navigation properties for efficient data loading
    /// </summary>
    public async Task<DetailedOrderDto?> GetOrderWithDetailsByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(referenceId))
        {
            throw new ArgumentException("ReferenceId cannot be null or empty", nameof(referenceId));
        }

        // Get order with all related entities using repository method with navigation properties
        var order = await _orderRepository.GetByReferenceIdWithDetailsAsync(referenceId, cancellationToken);

        if (order == null)
        {
            return null;
        }

        // Use the original FromOrder method since navigation properties are loaded
        return DetailedOrderDto.FromOrder(order);
    }

    /// <summary>
    /// AddOrderItem - adds an item to an existing order
    /// </summary>
    public async Task<OrderItemResponse> AddOrderItemAsync(AddOrderItemRequest request, CancellationToken cancellationToken = default)
    {
        // Validate order exists
        var order = await _orderRepository.GetByIdAsync(OrderId.From(request.OrderId), cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");
        }

        // Create the order item
        var orderItem = OrderItem.Create(
            OrderId.From(request.OrderId),
            ProductId.From(request.ProductId),
            request.Quantity,
            request.NetAmount,
            request.GrossAmount,
            request.Currency
        );

        // Save the order item
        await _orderItemRepository.AddAsync(orderItem, cancellationToken);
        await _orderItemRepository.SaveChangesAsync(cancellationToken);

        return OrderItemResponse.FromOrderItem(orderItem);
    }

    /// <summary>
    /// UpdateOrderItem - updates quantity and amounts for an order item
    /// </summary>
    public async Task<OrderItemResponse> UpdateOrderItemAsync(UpdateOrderItemRequest request, CancellationToken cancellationToken = default)
    {
        var orderItem = await _orderItemRepository.GetByIdAsync(OrderItemId.From(request.OrderItemId), cancellationToken);
        if (orderItem == null)
        {
            throw new InvalidOperationException($"OrderItem with ID {request.OrderItemId} not found");
        }

        // Update the order item
        orderItem.UpdateQuantityAndAmounts(request.Quantity, request.NetAmount, request.GrossAmount);

        // Save changes
        await _orderItemRepository.UpdateAsync(orderItem, cancellationToken);
        await _orderItemRepository.SaveChangesAsync(cancellationToken);

        return OrderItemResponse.FromOrderItem(orderItem);
    }

    /// <summary>
    /// RemoveOrderItem - removes an order item from an order
    /// </summary>
    public async Task RemoveOrderItemAsync(Guid orderItemId, CancellationToken cancellationToken = default)
    {
        var orderItem = await _orderItemRepository.GetByIdAsync(OrderItemId.From(orderItemId), cancellationToken);
        if (orderItem == null)
        {
            throw new InvalidOperationException($"OrderItem with ID {orderItemId} not found");
        }

        await _orderItemRepository.RemoveAsync(orderItem, cancellationToken);
        await _orderItemRepository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// GetOrderItems - gets all items for a specific order
    /// </summary>
    public async Task<IEnumerable<OrderItemResponse>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var orderItems = await _orderItemRepository.GetByOrderIdAsync(OrderId.From(orderId), cancellationToken);
        return orderItems.Select(OrderItemResponse.FromOrderItem);
    }

}
