using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Represents a loyalty transaction associated with an order
/// </summary>
public class OrderLoyalty
{
    /// <summary>
    /// Unique identifier for the loyalty transaction
    /// </summary>
    public LoyaltyTransactionId Id { get; private set; }

    /// <summary>
    /// The order this loyalty transaction belongs to
    /// </summary>
    public OrderId OrderId { get; private set; }

    /// <summary>
    /// Type of loyalty transaction (Earn or Burn)
    /// </summary>
    public LoyaltyTransactionType TransactionType { get; private set; }

    /// <summary>
    /// Number of points involved in the transaction
    /// </summary>
    public int Points { get; private set; }

    /// <summary>
    /// When the transaction occurred
    /// </summary>
    public DateTime TransactionDate { get; private set; }

    /// <summary>
    /// Description of the transaction
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Reference to any external loyalty system transaction ID
    /// </summary>
    public string? ExternalTransactionId { get; private set; }

    /// <summary>
    /// Navigation property back to the parent Order
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    // Private constructor for EF Core
    private OrderLoyalty()
    {
        Id = null!;
        OrderId = null!;
        Description = string.Empty;
    }

    /// <summary>
    /// Creates a new loyalty transaction for earning points
    /// </summary>
    public static OrderLoyalty CreateEarnTransaction(
        OrderId orderId,
        int points,
        string description,
        string? externalTransactionId = null)
    {
        if (points <= 0)
            throw new ArgumentException("Points must be positive for earn transactions", nameof(points));

        return new OrderLoyalty
        {
            Id = LoyaltyTransactionId.New(),
            OrderId = orderId,
            TransactionType = LoyaltyTransactionType.Earn,
            Points = points,
            TransactionDate = DateTime.UtcNow,
            Description = description,
            ExternalTransactionId = externalTransactionId
        };
    }

    /// <summary>
    /// Creates a new loyalty transaction for burning points
    /// </summary>
    public static OrderLoyalty CreateBurnTransaction(
        OrderId orderId,
        int points,
        string description,
        string? externalTransactionId = null)
    {
        if (points <= 0)
            throw new ArgumentException("Points must be positive for burn transactions", nameof(points));

        return new OrderLoyalty
        {
            Id = LoyaltyTransactionId.New(),
            OrderId = orderId,
            TransactionType = LoyaltyTransactionType.Burn,
            Points = points,
            TransactionDate = DateTime.UtcNow,
            Description = description,
            ExternalTransactionId = externalTransactionId
        };
    }
}

/// <summary>
/// Type of loyalty transaction
/// </summary>
public enum LoyaltyTransactionType
{
    /// <summary>
    /// Points are earned/credited
    /// </summary>
    Earn,

    /// <summary>
    /// Points are burned/debited
    /// </summary>
    Burn
}
