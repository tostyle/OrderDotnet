using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for OrderJourney entity
/// Handles state transition audit trail operations
/// </summary>
public interface IOrderJourneyRepository : IRepository<OrderJourney>
{
    /// <summary>
    /// Gets all journey records for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of order journey records ordered by transition date</returns>
    Task<IEnumerable<OrderJourney>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journey records for a specific order filtered by state
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="state">The state to filter by (either old or new state)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of journey records involving the specified state</returns>
    Task<IEnumerable<OrderJourney>> GetByOrderIdAndStateAsync(OrderId orderId, OrderState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journey records by old state for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="oldState">The old state to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of journey records with the specified old state</returns>
    Task<IEnumerable<OrderJourney>> GetByOrderIdAndOldStateAsync(OrderId orderId, OrderState oldState, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journey records by new state for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="newState">The new state to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of journey records with the specified new state</returns>
    Task<IEnumerable<OrderJourney>> GetByOrderIdAndNewStateAsync(OrderId orderId, OrderState newState, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest journey record for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The most recent journey record or null if none exist</returns>
    Task<OrderJourney?> GetLatestByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journey records within a date range for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="fromDate">Start date (inclusive)</param>
    /// <param name="toDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of journey records within the date range</returns>
    Task<IEnumerable<OrderJourney>> GetByOrderIdAndDateRangeAsync(
        OrderId orderId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journey records for multiple orders
    /// </summary>
    /// <param name="orderIds">Collection of order identifiers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of journey records for all specified orders</returns>
    Task<IEnumerable<OrderJourney>> GetByOrderIdsAsync(IEnumerable<OrderId> orderIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journey records by transition pattern (old state -> new state)
    /// </summary>
    /// <param name="oldState">The old state</param>
    /// <param name="newState">The new state</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of journey records matching the transition pattern</returns>
    Task<IEnumerable<OrderJourney>> GetByTransitionPatternAsync(
        OrderState oldState,
        OrderState newState,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts journey records for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of journey records for the order</returns>
    Task<int> CountByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a specific state transition exists for an order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="oldState">The old state</param>
    /// <param name="newState">The new state</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the transition exists</returns>
    Task<bool> HasTransitionAsync(OrderId orderId, OrderState oldState, OrderState newState, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the order state timeline (all state changes in chronological order)
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Ordered collection of journey records representing the state timeline</returns>
    Task<IEnumerable<OrderJourney>> GetStateTimelineAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the data store
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
