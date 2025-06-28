using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// OrderAggregate partial class containing stock reservation logic
/// </summary>
public partial class OrderAggregate
{
    /// <summary>
    /// Reserves stock for the order
    /// </summary>
    /// <param name="productId">The product to reserve</param>
    /// <param name="quantity">Quantity to reserve</param>
    /// <returns>The stock reservation ID</returns>
    public StockReservationId ReserveStock(ProductId productId, int quantity)
    {
        ValidateStockReservationRequest(quantity);
        ValidateOrderStateForStockReservation();

        var stockReservation = OrderStock.Create(_order.Id, productId, quantity);
        _stockReservations.Add(stockReservation);

        return stockReservation.Id;
    }

    /// <summary>
    /// Reserves multiple items for the order
    /// </summary>
    /// <param name="items">List of products and quantities to reserve</param>
    /// <returns>Dictionary of product IDs to reservation IDs</returns>
    public Dictionary<ProductId, StockReservationId> ReserveBulkStock(IEnumerable<(ProductId ProductId, int Quantity)> items)
    {
        ValidateOrderStateForStockReservation();

        var reservations = new Dictionary<ProductId, StockReservationId>();

        foreach (var (productId, quantity) in items)
        {
            var reservationId = ReserveStock(productId, quantity);
            reservations[productId] = reservationId;
        }

        return reservations;
    }

    /// <summary>
    /// Confirms a stock reservation (moves from reserved to confirmed)
    /// </summary>
    /// <param name="reservationId">The reservation to confirm</param>
    public void ConfirmStockReservation(StockReservationId reservationId)
    {
        var reservation = GetStockReservationById(reservationId);
        ValidateReservationForConfirmation(reservation);

        reservation.Confirm();

        // If all stock is confirmed, could transition order state
        if (AllStockConfirmed() && _order.OrderState == OrderState.Paid)
        {
            // Could transition to a "Processing" or "Ready to Pack" state
            // TransitionTo(OrderState.Processing, "All stock confirmed");
        }
    }

    /// <summary>
    /// Confirms all stock reservations for the order
    /// </summary>
    public void ConfirmAllStockReservations()
    {
        var reservationsToConfirm = _stockReservations
            .Where(r => r.Status == ReservationStatus.Reserved)
            .ToList();

        foreach (var reservation in reservationsToConfirm)
        {
            reservation.Confirm();
        }

        if (AllStockConfirmed() && _order.OrderState == OrderState.Paid)
        {
            // Could transition order state when all stock is confirmed
            // TransitionTo(OrderState.Processing, "All stock confirmed");
        }
    }

    /// <summary>
    /// Releases a stock reservation
    /// </summary>
    /// <param name="reservationId">The reservation to release</param>
    /// <param name="reason">Reason for release</param>
    public void ReleaseStockReservation(StockReservationId reservationId, string? reason = null)
    {
        var reservation = GetStockReservationById(reservationId);
        ValidateReservationForRelease(reservation);

        reservation.Release(reason ?? "Stock released");
    }

    /// <summary>
    /// Releases all stock reservations (e.g., when order is cancelled)
    /// </summary>
    /// <param name="reason">Reason for releasing all stock</param>
    public void ReleaseAllStockReservations(string reason = "Order cancelled - releasing all stock")
    {
        var activeReservations = _stockReservations
            .Where(r => r.Status == ReservationStatus.Reserved || r.Status == ReservationStatus.Confirmed)
            .ToList();

        foreach (var reservation in activeReservations)
        {
            reservation.Release(reason);
        }
    }

    /// <summary>
    /// Updates the quantity of an existing reservation
    /// </summary>
    /// <param name="reservationId">The reservation to update</param>
    /// <param name="newQuantity">The new quantity</param>
    public void UpdateStockReservationQuantity(StockReservationId reservationId, int newQuantity)
    {
        ValidateStockReservationRequest(newQuantity);

        var reservation = GetStockReservationById(reservationId);
        ValidateReservationForUpdate(reservation);

        reservation.UpdateQuantity(newQuantity);
    }

    /// <summary>
    /// Gets the total quantity reserved for a specific product
    /// </summary>
    /// <param name="productId">The product to check</param>
    /// <returns>Total quantity reserved</returns>
    public int GetTotalReservedQuantity(ProductId productId)
    {
        return _stockReservations
            .Where(r => r.ProductId == productId &&
                       (r.Status == ReservationStatus.Reserved || r.Status == ReservationStatus.Confirmed))
            .Sum(r => r.QuantityReserved);
    }

    /// <summary>
    /// Checks if the order has sufficient stock reserved
    /// </summary>
    /// <returns>True if all required stock is reserved</returns>
    public bool HasSufficientStockReserved()
    {
        // This would need business logic to determine what "sufficient" means
        // For now, just check if we have any active reservations
        return _stockReservations.Any(r =>
            r.Status == ReservationStatus.Reserved || r.Status == ReservationStatus.Confirmed);
    }

    #region Stock Private Helpers

    private void ValidateStockReservationRequest(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }
    }

    private void ValidateOrderStateForStockReservation()
    {
        if (_order.OrderState == OrderState.Cancelled || _order.OrderState == OrderState.Refunded)
        {
            throw new InvalidOperationException("Cannot reserve stock for cancelled or refunded orders");
        }
    }

    private OrderStock GetStockReservationById(StockReservationId reservationId)
    {
        var reservation = _stockReservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation == null)
        {
            throw new InvalidOperationException($"Stock reservation {reservationId} not found");
        }
        return reservation;
    }

    private void ValidateReservationForConfirmation(OrderStock reservation)
    {
        if (reservation.Status != ReservationStatus.Reserved)
        {
            throw new InvalidOperationException($"Stock reservation {reservation.Id} is not in reserved status");
        }
    }

    private void ValidateReservationForRelease(OrderStock reservation)
    {
        if (reservation.Status == ReservationStatus.Released)
        {
            throw new InvalidOperationException($"Stock reservation {reservation.Id} is already released");
        }
    }

    private void ValidateReservationForUpdate(OrderStock reservation)
    {
        if (reservation.Status == ReservationStatus.Released)
        {
            throw new InvalidOperationException($"Cannot update released stock reservation {reservation.Id}");
        }
    }

    private bool AllStockConfirmed()
    {
        return _stockReservations.Any() &&
               _stockReservations.All(r =>
                   r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Released);
    }

    #endregion
}