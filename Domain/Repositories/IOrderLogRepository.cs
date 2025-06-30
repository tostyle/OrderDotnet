using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for OrderLog entity
/// Handles order logging and audit trail operations
/// </summary>
public interface IOrderLogRepository : IRepository<OrderLog>
{
    /// <summary>
    /// Gets all log records for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of order log records ordered by action date</returns>
    Task<IEnumerable<OrderLog>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets log records for a specific order filtered by action type
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="actionType">The action type to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of log records with the specified action type</returns>
    Task<IEnumerable<OrderLog>> GetByOrderIdAndActionTypeAsync(OrderId orderId, string actionType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets log records for a specific order filtered by log level
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="level">The log level to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of log records with the specified level</returns>
    Task<IEnumerable<OrderLog>> GetByOrderIdAndLevelAsync(OrderId orderId, LogLevel level, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets error log records for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of error log records</returns>
    Task<IEnumerable<OrderLog>> GetErrorsByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets log records within a date range for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="fromDate">Start date (inclusive)</param>
    /// <param name="toDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of log records within the date range</returns>
    Task<IEnumerable<OrderLog>> GetByOrderIdAndDateRangeAsync(
        OrderId orderId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets log records for multiple orders
    /// </summary>
    /// <param name="orderIds">Collection of order identifiers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of log records for all specified orders</returns>
    Task<IEnumerable<OrderLog>> GetByOrderIdsAsync(IEnumerable<OrderId> orderIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets log records by action type across all orders
    /// </summary>
    /// <param name="actionType">The action type to filter by</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of log records with the specified action type</returns>
    Task<IEnumerable<OrderLog>> GetByActionTypeAsync(
        string actionType,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets log records by performer
    /// </summary>
    /// <param name="performedBy">Who performed the actions</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of log records performed by the specified user/system</returns>
    Task<IEnumerable<OrderLog>> GetByPerformedByAsync(
        string performedBy,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest log record for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The most recent log record or null if none exist</returns>
    Task<OrderLog?> GetLatestByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts log records for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of log records for the order</returns>
    Task<int> CountByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts error log records for a specific order
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of error log records for the order</returns>
    Task<int> CountErrorsByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets log records with pagination
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="skip">Number of records to skip</param>
    /// <param name="take">Number of records to take</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of log records</returns>
    Task<IEnumerable<OrderLog>> GetPagedByOrderIdAsync(
        OrderId orderId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches log records by description content
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="searchTerm">Search term to match in description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of log records matching the search term</returns>
    Task<IEnumerable<OrderLog>> SearchByDescriptionAsync(
        OrderId orderId,
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets aggregated statistics for log records
    /// </summary>
    /// <param name="orderId">The order identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with log statistics (count by action type, level, etc.)</returns>
    Task<Dictionary<string, int>> GetStatisticsByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the data store
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
