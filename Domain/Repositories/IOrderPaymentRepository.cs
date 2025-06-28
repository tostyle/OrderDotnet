using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for OrderPayment entity
/// </summary>
public interface IOrderPaymentRepository
{
    /// <summary>
    /// Gets a payment by its unique identifier
    /// </summary>
    Task<OrderPayment?> GetByIdAsync(PaymentId paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all payments for a specific order
    /// </summary>
    Task<IEnumerable<OrderPayment>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all payments with optional pagination
    /// </summary>
    Task<IEnumerable<OrderPayment>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new payment to the repository
    /// </summary>
    Task AddAsync(OrderPayment payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing payment
    /// </summary>
    Task UpdateAsync(OrderPayment payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a payment from the repository
    /// </summary>
    Task RemoveAsync(OrderPayment payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments by status
    /// </summary>
    Task<IEnumerable<OrderPayment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the underlying data store
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
