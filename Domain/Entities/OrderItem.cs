using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Represents an item within an order
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Unique identifier for the order item
    /// </summary>
    public OrderItemId Id { get; private set; }

    /// <summary>
    /// The order this item belongs to
    /// </summary>
    public OrderId OrderId { get; private set; }

    /// <summary>
    /// Product identifier for the item
    /// </summary>
    public ProductId ProductId { get; private set; }

    /// <summary>
    /// Quantity of the product
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Net amount (before tax) for this item
    /// </summary>
    public decimal NetAmount { get; private set; }

    /// <summary>
    /// Gross amount (including tax) for this item
    /// </summary>
    public decimal GrossAmount { get; private set; }

    /// <summary>
    /// Currency code (default: THB)
    /// </summary>
    public string Currency { get; private set; }

    /// <summary>
    /// Navigation property back to the parent Order
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    // Private constructor for EF Core
    private OrderItem()
    {
        Id = null!;
        OrderId = null!;
        ProductId = null!;
        Currency = string.Empty;
    }

    /// <summary>
    /// Creates a new order item
    /// </summary>
    /// <param name="orderId">The order this item belongs to</param>
    /// <param name="productId">The product identifier</param>
    /// <param name="quantity">Quantity of the product</param>
    /// <param name="netAmount">Net amount before tax</param>
    /// <param name="grossAmount">Gross amount including tax</param>
    /// <param name="currency">Currency code (default: THB)</param>
    /// <returns>New OrderItem instance</returns>
    public static OrderItem Create(
        OrderId orderId,
        ProductId productId,
        int quantity,
        decimal netAmount,
        decimal grossAmount,
        string currency = "THB")
    {
        if (orderId == null)
            throw new ArgumentNullException(nameof(orderId));
        if (productId == null)
            throw new ArgumentNullException(nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        if (netAmount < 0)
            throw new ArgumentException("Net amount cannot be negative", nameof(netAmount));
        if (grossAmount < 0)
            throw new ArgumentException("Gross amount cannot be negative", nameof(grossAmount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        return new OrderItem
        {
            Id = OrderItemId.New(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            NetAmount = netAmount,
            GrossAmount = grossAmount,
            Currency = currency.ToUpperInvariant()
        };
    }

    /// <summary>
    /// Updates the quantity and amounts for this order item
    /// </summary>
    /// <param name="quantity">New quantity</param>
    /// <param name="netAmount">New net amount</param>
    /// <param name="grossAmount">New gross amount</param>
    public void UpdateQuantityAndAmounts(int quantity, decimal netAmount, decimal grossAmount)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        if (netAmount < 0)
            throw new ArgumentException("Net amount cannot be negative", nameof(netAmount));
        if (grossAmount < 0)
            throw new ArgumentException("Gross amount cannot be negative", nameof(grossAmount));

        Quantity = quantity;
        NetAmount = netAmount;
        GrossAmount = grossAmount;
    }

    /// <summary>
    /// Calculates the total net amount for this item (quantity * netAmount)
    /// </summary>
    public decimal CalculateTotalNetAmount() => Quantity * NetAmount;

    /// <summary>
    /// Calculates the total gross amount for this item (quantity * grossAmount)
    /// </summary>
    public decimal CalculateTotalGrossAmount() => Quantity * GrossAmount;
}
