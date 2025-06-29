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

    public OrderService(
        IOrderRepository orderRepository,
        IOrderPaymentRepository paymentRepository,
        IOrderLoyaltyRepository loyaltyRepository,
        IOrderStockRepository stockRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _loyaltyRepository = loyaltyRepository ?? throw new ArgumentNullException(nameof(loyaltyRepository));
        _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
    }

    /// <summary>
    /// 1. InitialOrder - create DTO, init Order, OrderPayment with Payment Pending status and save it to each repo model
    /// Idempotent by ReferenceId - returns existing order if already exists
    /// </summary>
    public async Task<InitialOrderResponse> InitialOrderAsync(InitialOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ReferenceId))
        {
            throw new ArgumentException("ReferenceId is required for idempotent operation", nameof(request.ReferenceId));
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
                PaymentStatus: firstPayment.Status.ToString()
            );
        }

        // Create new order if not exists
        var order = Order.Create(request.ReferenceId);

        // Create OrderAggregate to manage business logic
        var orderAggregate = OrderAggregate.FromExistingOrder(order);

        // Use domain factory method for payment method creation
        var paymentMethod = PaymentMethod.FromString(request.PaymentMethod);
        var paymentId = orderAggregate.ProcessPayment(paymentMethod, request.PaymentAmount, request.Currency);

        // Get the payment entity from the aggregate
        var payment = orderAggregate.Payments.First(p => p.Id == paymentId);

        // Save order to repository
        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        // Save payment to repository
        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        return new InitialOrderResponse(
            OrderId: order.Id.Value,
            ReferenceId: order.ReferenceId,
            PaymentId: payment.Id.Value,
            PaymentStatus: payment.Status.ToString()
        );
    }

    /// <summary>
    /// 2. ReserveStock - create DTO, find Order By Id and then call reservestock method in OrderAggregate save to repository
    /// </summary>
    public async Task<ReserveStockResponse> ReserveStockAsync(ReserveStockRequest request, CancellationToken cancellationToken = default)
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

        // Call reserve stock method in OrderAggregate
        var productId = ProductId.From(request.ProductId);
        var reservationId = orderAggregate.ReserveStock(productId, request.Quantity);

        // Get the stock reservation from the aggregate
        var stockReservation = orderAggregate.StockReservations.First(s => s.Id == reservationId);

        // Save stock reservation to repository
        await _stockRepository.AddAsync(stockReservation, cancellationToken);
        await _stockRepository.SaveChangesAsync(cancellationToken);

        return new ReserveStockResponse(
            ReservationId: stockReservation.Id.Value,
            ProductId: stockReservation.ProductId.Value,
            Quantity: stockReservation.QuantityReserved,
            Status: stockReservation.Status.ToString()
        );
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
    /// GetOrderWithDetails - retrieve order with all related entities (Payment, LoyaltyTransactions, StockReservations)
    /// using navigation properties for efficient data loading
    /// </summary>
    public async Task<DetailedOrderDto?> GetOrderWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(OrderId.From(orderId), cancellationToken);

        if (order == null)
        {
            return null;
        }

        return DetailedOrderDto.FromOrder(order);
    }

    /// <summary>
    /// GetOrderWithDetailsByReferenceId - retrieve order with all related entities by reference ID
    /// using navigation properties for efficient data loading
    /// </summary>
    public async Task<DetailedOrderDto?> GetOrderWithDetailsByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(referenceId))
        {
            throw new ArgumentException("ReferenceId cannot be null or empty", nameof(referenceId));
        }

        var order = await _orderRepository.GetByReferenceIdWithDetailsAsync(referenceId, cancellationToken);

        if (order == null)
        {
            return null;
        }

        return DetailedOrderDto.FromOrder(order);
    }

}
