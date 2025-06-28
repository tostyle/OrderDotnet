using Domain.Entities;
using Domain.ValueObjects;
using Application.DTOs;

namespace Application.Extensions;

/// <summary>
/// Extension methods for mapping between domain entities and DTOs
/// </summary>
public static class OrderMappingExtensions
{
    /// <summary>
    /// Maps Order entity to OrderDto
    /// </summary>
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto(
            Id: order.Id.Value,
            ReferenceId: order.ReferenceId,
            OrderState: order.OrderState.ToString(),
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt,
            Version: order.Version,
            WorkflowId: order.WorkflowId
        );
    }

    /// <summary>
    /// Maps collection of Order entities to DTOs
    /// </summary>
    public static IEnumerable<OrderDto> ToDtos(this IEnumerable<Order> orders)
    {
        return orders.Select(order => order.ToDto());
    }

    /// <summary>
    /// Maps OrderId from Guid
    /// </summary>
    public static OrderId ToOrderId(this Guid id)
    {
        return OrderId.From(id);
    }

    /// <summary>
    /// Creates OrderListResponse from orders collection
    /// </summary>
    public static OrderListResponse ToOrderListResponse(
        this IEnumerable<Order> orders,
        int totalCount,
        int skip,
        int take)
    {
        return new OrderListResponse(
            Orders: orders.ToDtos(),
            TotalCount: totalCount,
            Skip: skip,
            Take: take
        );
    }
}
