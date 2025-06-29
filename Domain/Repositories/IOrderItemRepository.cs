using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for OrderItem entity
/// Extends IRepository for basic CRUD operations and adds domain-specific queries
/// </summary>
public interface IOrderItemRepository : IRepository<OrderItem>
{
    /// <summary>
    /// Gets all order items for a specific order
    /// </summary>
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific order item by ID
    /// </summary>
    Task<OrderItem?> GetByIdAsync(OrderItemId orderItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets order items for multiple orders
    /// </summary>
    Task<IEnumerable<OrderItem>> GetByOrderIdsAsync(IEnumerable<OrderId> orderIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets order items by product ID across all orders
    /// </summary>
    Task<IEnumerable<OrderItem>> GetByProductIdAsync(ProductId productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new order item
    /// </summary>
    Task AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing order item
    /// </summary>
    Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an order item
    /// </summary>
    Task RemoveAsync(OrderItem orderItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple order items for an order
    /// </summary>
    Task RemoveByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the underlying data store
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
