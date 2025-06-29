using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// Order aggregate root - manages Order entity and related business logic
/// </summary>
public partial class OrderAggregate
{
    private readonly Order _order;
    private readonly List<OrderLoyalty> _loyaltyTransactions = new();
    private readonly List<OrderPayment> _payments = new();
    private readonly List<OrderStock> _stockReservations = new();

    /// <summary>
    /// Constructor for creating new aggregate with existing order
    /// </summary>
    public OrderAggregate(Order order)
    {
        _order = order ?? throw new ArgumentNullException(nameof(order));
    }

    /// <summary>
    /// Factory method to create a new order aggregate
    /// </summary>
    public static OrderAggregate CreateNew()
    {
        var order = Order.Create();
        return new OrderAggregate(order);
    }

    /// <summary>
    /// Factory method to create an aggregate from an existing order
    /// </summary>
    public static OrderAggregate FromExistingOrder(Order order)
    {
        return new OrderAggregate(order);
    }

    /// <summary>
    /// Factory method to create an aggregate from an existing order and optional related collections
    /// </summary>
    public static OrderAggregate From(
        Order order,
        List<OrderLoyalty>? loyaltyTransactions = null,
        List<OrderPayment>? payments = null,
        List<OrderStock>? stockReservations = null)
    {
        OrderAggregate aggregate = new OrderAggregate(order);
        aggregate._loyaltyTransactions.AddRange(loyaltyTransactions ?? Enumerable.Empty<OrderLoyalty>());
        aggregate._payments.AddRange(payments ?? Enumerable.Empty<OrderPayment>());
        aggregate._stockReservations.AddRange(stockReservations ?? Enumerable.Empty<OrderStock>());
        return aggregate;
    }

    /// <summary>
    /// Gets the underlying order entity
    /// </summary>
    public Order Order => _order;

    /// <summary>
    /// Read-only collection of loyalty transactions
    /// </summary>
    public IReadOnlyCollection<OrderLoyalty> LoyaltyTransactions => _loyaltyTransactions.AsReadOnly();

    /// <summary>
    /// Read-only collection of payments
    /// </summary>
    public IReadOnlyCollection<OrderPayment> Payments => _payments.AsReadOnly();

    /// <summary>
    /// Read-only collection of stock reservations
    /// </summary>
    public IReadOnlyCollection<OrderStock> StockReservations => _stockReservations.AsReadOnly();

    /// <summary>
    /// Gets the total amount of all successful payments
    /// </summary>
    public decimal TotalPaidAmount => _payments
        .Where(p => p.Status == PaymentStatus.Successful)
        .Sum(p => p.Amount);

    /// <summary>
    /// Gets the total loyalty points earned for this order
    /// </summary>
    public int TotalLoyaltyPointsEarned => _loyaltyTransactions
        .Where(t => t.TransactionType == LoyaltyTransactionType.Earn)
        .Sum(t => t.Points);

    /// <summary>
    /// Gets the total loyalty points burned for this order
    /// </summary>
    public int TotalLoyaltyPointsBurned => _loyaltyTransactions
        .Where(t => t.TransactionType == LoyaltyTransactionType.Burn)
        .Sum(t => t.Points);

    /// <summary>
    /// Checks if a state transition is valid using the existing validator
    /// </summary>
    public bool CanTransitionTo(OrderState newState)
    {
        return OrderTransitionValidator.IsValidTransition(_order.OrderState, newState);
    }

    /// <summary>
    /// Gets valid next states for the current order state
    /// </summary>
    public List<OrderState> GetValidNextStates()
    {
        // Get all possible states and filter for valid transitions
        var allStates = Enum.GetValues<OrderState>();
        var validNextStates = new List<OrderState>();

        foreach (var state in allStates)
        {
            if (CanTransitionTo(state))
            {
                validNextStates.Add(state);
            }
        }

        return validNextStates;
    }

    /// <summary>
    /// Transitions the order to a new state if valid
    /// </summary>
    public void TransitionTo(OrderState newState, string? reason = null)
    {
        if (!CanTransitionTo(newState))
        {
            throw new InvalidOperationException(
                $"Invalid state transition from {_order.OrderState} to {newState}. Reason: {reason}");
        }

        var previousState = _order.OrderState;
        _order.OrderState = newState;
        _order.UpdatedAt = DateTime.UtcNow;
        _order.Version++;

        // TODO: Raise domain events for state transition
        // RaiseDomainEvent(new OrderStateChangedEvent(_order.Id, previousState, newState, reason));
    }

    /// <summary>
    /// Checks if the order has any pending payments
    /// </summary>
    public bool HasPendingPayments() => _payments.Any(p => p.Status == PaymentStatus.Pending);

    /// <summary>
    /// Checks if the order is fully paid
    /// </summary>
    public bool IsFullyPaid(decimal orderAmount) => TotalPaidAmount >= orderAmount;

    /// <summary>
    /// Checks if all stock is reserved
    /// </summary>
    public bool HasStockReserved() => _stockReservations.Any(s =>
        s.Status == ReservationStatus.Reserved || s.Status == ReservationStatus.Confirmed);

    /// <summary>
    /// Validates business rules for the order
    /// </summary>
    public void ValidateBusinessRules()
    {
        // TODO: Implement comprehensive business rule validation
        // Examples:
        // - Order must have at least one stock reservation
        // - Payment amount must match order total
        // - Cannot cancel order if already completed
        // - etc.
    }
}