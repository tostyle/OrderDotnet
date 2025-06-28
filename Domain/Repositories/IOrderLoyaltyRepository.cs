using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for OrderLoyalty entity
/// </summary>
public interface IOrderLoyaltyRepository
{
    /// <summary>
    /// Gets a loyalty transaction by its unique identifier
    /// </summary>
    Task<OrderLoyalty?> GetByIdAsync(LoyaltyTransactionId transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all loyalty transactions for a specific order
    /// </summary>
    Task<IEnumerable<OrderLoyalty>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all loyalty transactions with optional pagination
    /// </summary>
    Task<IEnumerable<OrderLoyalty>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new loyalty transaction to the repository
    /// </summary>
    Task AddAsync(OrderLoyalty loyaltyTransaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing loyalty transaction
    /// </summary>
    Task UpdateAsync(OrderLoyalty loyaltyTransaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a loyalty transaction from the repository
    /// </summary>
    Task RemoveAsync(OrderLoyalty loyaltyTransaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets loyalty transactions by type
    /// </summary>
    Task<IEnumerable<OrderLoyalty>> GetByTypeAsync(LoyaltyTransactionType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets earned points total for an order
    /// </summary>
    Task<int> GetTotalEarnedPointsAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets burned points total for an order
    /// </summary>
    Task<int> GetTotalBurnedPointsAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the underlying data store
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
