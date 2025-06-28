using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Order aggregate
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets an order by its unique identifier
    /// </summary>
    Task<Order?> GetByIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all orders with optional pagination
    /// </summary>
    Task<IEnumerable<Order>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new order to the repository
    /// </summary>
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing order
    /// </summary>
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an order from the repository
    /// </summary>
    Task RemoveAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders by workflow ID
    /// </summary>
    Task<Order?> GetByWorkflowIdAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order by its reference ID
    /// </summary>
    Task<Order?> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the underlying data store
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
