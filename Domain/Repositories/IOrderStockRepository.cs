using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for OrderStock entity
/// </summary>
public interface IOrderStockRepository
{
    /// <summary>
    /// Gets a stock reservation by its unique identifier
    /// </summary>
    Task<OrderStock?> GetByIdAsync(StockReservationId reservationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all stock reservations for a specific order
    /// </summary>
    Task<IEnumerable<OrderStock>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all stock reservations with optional pagination
    /// </summary>
    Task<IEnumerable<OrderStock>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new stock reservation to the repository
    /// </summary>
    Task AddAsync(OrderStock stockReservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing stock reservation
    /// </summary>
    Task UpdateAsync(OrderStock stockReservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a stock reservation from the repository
    /// </summary>
    Task RemoveAsync(OrderStock stockReservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stock reservations by status
    /// </summary>
    Task<IEnumerable<OrderStock>> GetByStatusAsync(ReservationStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stock reservations by product
    /// </summary>
    Task<IEnumerable<OrderStock>> GetByProductIdAsync(ProductId productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing stock reservation by OrderId and ProductId for idempotency check
    /// </summary>
    Task<OrderStock?> GetByOrderIdAndProductIdAsync(OrderId orderId, ProductId productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total reserved quantity for a product
    /// </summary>
    Task<int> GetTotalReservedQuantityAsync(ProductId productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the underlying data store
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
