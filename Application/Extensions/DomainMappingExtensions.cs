using Domain.Entities;
using Application.DTOs;

namespace Application.Extensions;

/// <summary>
/// Extension methods for mapping domain entities to DTOs
/// Provides convenient, fluent mapping API
/// </summary>
public static class DomainMappingExtensions
{
    /// <summary>
    /// Converts Order entity to OrderDto
    /// </summary>
    /// <param name="order">The Order entity</param>
    /// <returns>OrderDto representation</returns>
    public static OrderDto ToDto(this Order order)
        => OrderDto.FromOrder(order);

    /// <summary>
    /// Converts OrderLoyalty entity to LoyaltyTransactionResponse
    /// </summary>
    /// <param name="loyalty">The OrderLoyalty entity</param>
    /// <returns>LoyaltyTransactionResponse representation</returns>
    public static LoyaltyTransactionResponse ToResponse(this OrderLoyalty loyalty)
        => LoyaltyTransactionResponse.FromOrderLoyalty(loyalty);

    /// <summary>
    /// Converts OrderPayment entity to ProcessPaymentResponse
    /// </summary>
    /// <param name="payment">The OrderPayment entity</param>
    /// <returns>ProcessPaymentResponse representation</returns>
    public static ProcessPaymentResponse ToResponse(this OrderPayment payment)
        => ProcessPaymentResponse.FromOrderPayment(payment);

    /// <summary>
    /// Converts OrderStock entity to ReserveStockResponse
    /// </summary>
    /// <param name="stock">The OrderStock entity</param>
    /// <returns>ReserveStockResponse representation</returns>
    public static ReserveStockResponse ToResponse(this OrderStock stock)
        => ReserveStockResponse.FromOrderStock(stock);

    /// <summary>
    /// Converts a collection of Order entities to OrderDto collection
    /// </summary>
    /// <param name="orders">Collection of Order entities</param>
    /// <returns>Collection of OrderDto representations</returns>
    public static IEnumerable<OrderDto> ToDtos(this IEnumerable<Order> orders)
        => orders.Select(ToDto);

    /// <summary>
    /// Converts a collection of OrderLoyalty entities to LoyaltyTransactionResponse collection
    /// </summary>
    /// <param name="loyalties">Collection of OrderLoyalty entities</param>
    /// <returns>Collection of LoyaltyTransactionResponse representations</returns>
    public static IEnumerable<LoyaltyTransactionResponse> ToResponses(this IEnumerable<OrderLoyalty> loyalties)
        => loyalties.Select(ToResponse);

    /// <summary>
    /// Converts a collection of OrderPayment entities to ProcessPaymentResponse collection
    /// </summary>
    /// <param name="payments">Collection of OrderPayment entities</param>
    /// <returns>Collection of ProcessPaymentResponse representations</returns>
    public static IEnumerable<ProcessPaymentResponse> ToResponses(this IEnumerable<OrderPayment> payments)
        => payments.Select(ToResponse);

    /// <summary>
    /// Converts a collection of OrderStock entities to ReserveStockResponse collection
    /// </summary>
    /// <param name="stocks">Collection of OrderStock entities</param>
    /// <returns>Collection of ReserveStockResponse representations</returns>
    public static IEnumerable<ReserveStockResponse> ToResponses(this IEnumerable<OrderStock> stocks)
        => stocks.Select(ToResponse);
}
