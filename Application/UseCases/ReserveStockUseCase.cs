using Application.DTOs;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;

namespace Application.UseCases;

/// <summary>
/// Use case for handling stock reservation - 6th Iteration Implementation
/// Follows clean architecture principles and implements idempotent stock reservation
/// Input: { orderId, productId }
/// Flow: Query OrderItem by productId -> Create OrderStock record from orderItem info
/// </summary>
public class ReserveStockUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IOrderStockRepository _stockRepository;

    public ReserveStockUseCase(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IOrderStockRepository stockRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
        _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
    }

    /// <summary>
    /// Executes the stock reservation use case following 6th iteration requirements
    /// Flow: 1. Validate input -> 2. Check idempotency -> 3. Query OrderItem -> 4. Create OrderStock
    /// </summary>
    /// <param name="request">The stock reservation request with orderId and productId</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The stock reservation response</returns>
    public async Task<ReserveStockResponse> ExecuteAsync(ReserveStockRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request
        if (request.OrderId == Guid.Empty)
        {
            throw new ArgumentException("OrderId is required", nameof(request.OrderId));
        }

        if (request.ProductId == Guid.Empty)
        {
            throw new ArgumentException("ProductId is required", nameof(request.ProductId));
        }

        var orderId = OrderId.From(request.OrderId);
        var productId = ProductId.From(request.ProductId);

        // Check if Order exists
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {request.OrderId} not found");
        }

        // Check if stock already reserved for idempotency
        var existingReservation = await _stockRepository.GetByOrderIdAndProductIdAsync(orderId, productId, cancellationToken);
        if (existingReservation is not null)
        {
            // Already reserved, return existing record (idempotent behavior)
            return ReserveStockResponse.FromOrderStock(existingReservation, isAlreadyReserved: true);
        }

        // Query OrderItem by productId to get quantity and other info
        var orderItems = await _orderItemRepository.GetByOrderIdAsync(orderId, cancellationToken);
        var orderItem = orderItems.FirstOrDefault(item => item.ProductId == productId);

        if (orderItem == null)
        {
            throw new InvalidOperationException($"OrderItem with ProductId {request.ProductId} not found in Order {request.OrderId}");
        }

        // Create OrderStock record from orderItem info
        var stockReservation = OrderStock.Create(
            orderId: orderId,
            productId: productId,
            quantity: orderItem.Quantity
        );

        // Set ReservationStatus = 'Reserved' (already set by default in Create method)
        // Save to repository
        await _stockRepository.AddAsync(stockReservation, cancellationToken);
        await _stockRepository.SaveChangesAsync(cancellationToken);

        return ReserveStockResponse.FromOrderStock(stockReservation, isAlreadyReserved: false);
    }
}
