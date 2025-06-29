using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Represents a stock reservation for an order
/// </summary>
public class OrderStock
{
    /// <summary>
    /// Unique identifier for the stock reservation
    /// </summary>
    public StockReservationId Id { get; private set; }

    /// <summary>
    /// The order this stock reservation belongs to
    /// </summary>
    public OrderId OrderId { get; private set; }

    /// <summary>
    /// Product identifier being reserved
    /// </summary>
    public ProductId ProductId { get; private set; }

    /// <summary>
    /// Quantity of the product reserved
    /// </summary>
    public int QuantityReserved { get; private set; }

    /// <summary>
    /// When the reservation was made
    /// </summary>
    public DateTime ReservationDate { get; private set; }

    /// <summary>
    /// When the reservation expires (if applicable)
    /// </summary>
    public DateTime? ExpirationDate { get; private set; }

    /// <summary>
    /// Current status of the reservation
    /// </summary>
    public ReservationStatus Status { get; private set; }

    /// <summary>
    /// External reference to inventory system
    /// </summary>
    public string? ExternalReservationId { get; private set; }

    /// <summary>
    /// Additional notes about the reservation
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Navigation property back to the parent Order
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    // Private constructor for EF Core
    private OrderStock()
    {
        Id = null!;
        OrderId = null!;
        ProductId = null!;
    }

    /// <summary>
    /// Creates a new stock reservation
    /// </summary>
    public static OrderStock Create(
        OrderId orderId,
        ProductId productId,
        int quantity,
        TimeSpan? reservationDuration = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var now = DateTime.UtcNow;
        DateTime? expiration = reservationDuration.HasValue ? now.Add(reservationDuration.Value) : null;

        return new OrderStock
        {
            Id = StockReservationId.New(),
            OrderId = orderId,
            ProductId = productId,
            QuantityReserved = quantity,
            ReservationDate = now,
            ExpirationDate = expiration,
            Status = ReservationStatus.Reserved
        };
    }

    /// <summary>
    /// Confirms the reservation (typically when order is paid)
    /// </summary>
    public void Confirm(string? notes = null)
    {
        if (Status != ReservationStatus.Reserved)
            throw new InvalidOperationException($"Cannot confirm reservation. Current status: {Status}");

        Status = ReservationStatus.Confirmed;
        Notes = notes;
    }

    /// <summary>
    /// Fulfills the reservation (stock is allocated)
    /// </summary>
    public void Fulfill(string? externalReservationId = null, string? notes = null)
    {
        if (Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException($"Cannot fulfill reservation. Current status: {Status}");

        Status = ReservationStatus.Fulfilled;
        ExternalReservationId = externalReservationId;
        Notes = notes;
    }

    /// <summary>
    /// Releases the reservation (stock becomes available again)
    /// </summary>
    public void Release(string? reason = null)
    {
        if (Status == ReservationStatus.Fulfilled)
            throw new InvalidOperationException("Cannot release fulfilled reservation");

        Status = ReservationStatus.Released;
        Notes = reason;
    }

    /// <summary>
    /// Checks if the reservation has expired
    /// </summary>
    public bool IsExpired()
    {
        return ExpirationDate.HasValue && DateTime.UtcNow > ExpirationDate.Value;
    }

    /// <summary>
    /// Updates the quantity reserved
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (Status != ReservationStatus.Reserved)
            throw new InvalidOperationException($"Cannot update quantity. Current status: {Status}");

        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

        QuantityReserved = newQuantity;
    }
}

/// <summary>
/// Stock reservation status enumeration
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// Stock is reserved but not yet confirmed
    /// </summary>
    Reserved,

    /// <summary>
    /// Reservation is confirmed (typically after payment)
    /// </summary>
    Confirmed,

    /// <summary>
    /// Stock has been fulfilled/allocated
    /// </summary>
    Fulfilled,

    /// <summary>
    /// Reservation has been released
    /// </summary>
    Released,

    /// <summary>
    /// Reservation has expired
    /// </summary>
    Expired
}
