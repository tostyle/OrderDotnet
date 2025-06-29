using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// OrderAggregate partial class for managing OrderItem operations
/// </summary>
public partial class OrderAggregate
{
    private readonly List<OrderItem> _orderItems = new();

    /// <summary>
    /// Gets read-only collection of order items
    /// </summary>
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    /// <summary>
    /// Adds an item to the order
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="quantity">Quantity of the product</param>
    /// <param name="netAmount">Net amount per unit</param>
    /// <param name="grossAmount">Gross amount per unit</param>
    /// <param name="currency">Currency code</param>
    /// <returns>The created OrderItem</returns>
    public OrderItem AddItem(ProductId productId, int quantity, decimal netAmount, decimal grossAmount, string currency = "THB")
    {
        if (productId == null)
            throw new ArgumentNullException(nameof(productId));

        // Check if item with same product already exists
        var existingItem = _orderItems.FirstOrDefault(oi => oi.ProductId == productId);
        if (existingItem != null)
        {
            // Update existing item quantity and amounts
            var newQuantity = existingItem.Quantity + quantity;
            existingItem.UpdateQuantityAndAmounts(newQuantity, netAmount, grossAmount);
            return existingItem;
        }

        // Create new order item
        var orderItem = OrderItem.Create(_order.Id, productId, quantity, netAmount, grossAmount, currency);
        _orderItems.Add(orderItem);

        return orderItem;
    }

    /// <summary>
    /// Updates an existing order item
    /// </summary>
    /// <param name="orderItemId">Order item identifier</param>
    /// <param name="quantity">New quantity</param>
    /// <param name="netAmount">New net amount per unit</param>
    /// <param name="grossAmount">New gross amount per unit</param>
    /// <returns>The updated OrderItem</returns>
    /// <exception cref="InvalidOperationException">When order item is not found</exception>
    public OrderItem UpdateItem(OrderItemId orderItemId, int quantity, decimal netAmount, decimal grossAmount)
    {
        var orderItem = _orderItems.FirstOrDefault(oi => oi.Id == orderItemId);
        if (orderItem == null)
            throw new InvalidOperationException($"Order item with ID {orderItemId} not found in this order");

        orderItem.UpdateQuantityAndAmounts(quantity, netAmount, grossAmount);
        return orderItem;
    }

    /// <summary>
    /// Removes an item from the order
    /// </summary>
    /// <param name="orderItemId">Order item identifier</param>
    /// <exception cref="InvalidOperationException">When order item is not found</exception>
    public void RemoveItem(OrderItemId orderItemId)
    {
        var orderItem = _orderItems.FirstOrDefault(oi => oi.Id == orderItemId);
        if (orderItem == null)
            throw new InvalidOperationException($"Order item with ID {orderItemId} not found in this order");

        _orderItems.Remove(orderItem);
    }

    /// <summary>
    /// Removes an item by product ID
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <exception cref="InvalidOperationException">When product is not found in order</exception>
    public void RemoveItemByProduct(ProductId productId)
    {
        var orderItem = _orderItems.FirstOrDefault(oi => oi.ProductId == productId);
        if (orderItem == null)
            throw new InvalidOperationException($"Product with ID {productId} not found in this order");

        _orderItems.Remove(orderItem);
    }

    /// <summary>
    /// Gets the total net amount for all order items
    /// </summary>
    public decimal GetTotalNetAmount()
    {
        return _orderItems.Sum(oi => oi.CalculateTotalNetAmount());
    }

    /// <summary>
    /// Gets the total gross amount for all order items
    /// </summary>
    public decimal GetTotalGrossAmount()
    {
        return _orderItems.Sum(oi => oi.CalculateTotalGrossAmount());
    }

    /// <summary>
    /// Gets the total quantity of all items in the order
    /// </summary>
    public int GetTotalItemCount()
    {
        return _orderItems.Sum(oi => oi.Quantity);
    }

    /// <summary>
    /// Checks if the order has any items
    /// </summary>
    public bool HasItems()
    {
        return _orderItems.Any();
    }

    /// <summary>
    /// Gets an order item by product ID
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <returns>OrderItem if found, null otherwise</returns>
    public OrderItem? GetItemByProduct(ProductId productId)
    {
        return _orderItems.FirstOrDefault(oi => oi.ProductId == productId);
    }

    /// <summary>
    /// Clears all items from the order
    /// </summary>
    public void ClearItems()
    {
        _orderItems.Clear();
    }
}
