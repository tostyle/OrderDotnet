using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// OrderAggregate partial class containing loyalty burning logic
/// </summary>
public partial class OrderAggregate
{
    /// <summary>
    /// Records loyalty points burned for this order (used as discount/payment)
    /// </summary>
    /// <param name="points">Number of points to burn</param>
    /// <param name="description">Description of the burn transaction</param>
    /// <param name="externalTransactionId">External system transaction reference</param>
    /// <returns>The loyalty transaction ID</returns>
    public LoyaltyTransactionId BurnLoyaltyPoints(int points, string? description = null, string? externalTransactionId = null)
    {
        ValidateLoyaltyBurnRequest(points);
        ValidateOrderStateForLoyaltyBurn();

        var loyaltyTransaction = OrderLoyalty.CreateBurnTransaction(
            _order.Id,
            points,
            description ?? $"Points burned for order {_order.Id}",
            externalTransactionId);

        _loyaltyTransactions.Add(loyaltyTransaction);

        return loyaltyTransaction.Id;
    }

    /// <summary>
    /// Burns points as a discount applied to the order
    /// </summary>
    /// <param name="points">Number of points to burn</param>
    /// <param name="pointValue">Value of each point in currency (e.g., $0.01 per point)</param>
    /// <returns>The discount amount applied</returns>
    public decimal BurnPointsAsDiscount(int points, decimal pointValue = 0.01m)
    {
        ValidateLoyaltyBurnRequest(points);
        ValidateOrderStateForLoyaltyBurn();

        if (pointValue <= 0)
            throw new ArgumentException("Point value must be positive", nameof(pointValue));

        var discountAmount = points * pointValue;

        BurnLoyaltyPoints(points, $"Points burned as ${discountAmount:F2} discount");

        return discountAmount;
    }

    /// <summary>
    /// Calculates the maximum points that can be burned for this order
    /// </summary>
    /// <param name="orderAmount">Total order amount</param>
    /// <param name="pointValue">Value of each point</param>
    /// <param name="maxDiscountPercentage">Maximum percentage of order that can be discounted (default 50%)</param>
    /// <returns>Maximum points that can be burned</returns>
    public int CalculateMaxPointsToBurn(decimal orderAmount, decimal pointValue = 0.01m, decimal maxDiscountPercentage = 0.5m)
    {
        if (orderAmount <= 0)
            throw new ArgumentException("Order amount must be positive", nameof(orderAmount));

        if (pointValue <= 0)
            throw new ArgumentException("Point value must be positive", nameof(pointValue));

        if (maxDiscountPercentage <= 0 || maxDiscountPercentage > 1)
            throw new ArgumentException("Max discount percentage must be between 0 and 1", nameof(maxDiscountPercentage));

        var maxDiscountAmount = orderAmount * maxDiscountPercentage;
        return (int)Math.Floor(maxDiscountAmount / pointValue);
    }

    /// <summary>
    /// Reverses a loyalty burn transaction (e.g., if order is cancelled)
    /// </summary>
    /// <param name="originalBurnTransactionId">The original burn transaction to reverse</param>
    /// <returns>The reversal transaction ID</returns>
    public LoyaltyTransactionId ReverseLoyaltyBurn(LoyaltyTransactionId originalBurnTransactionId)
    {
        var originalTransaction = _loyaltyTransactions.FirstOrDefault(t =>
            t.Id == originalBurnTransactionId && t.TransactionType == LoyaltyTransactionType.Burn);

        if (originalTransaction == null)
        {
            throw new InvalidOperationException($"Original burn transaction {originalBurnTransactionId} not found");
        }

        // Create an earn transaction to reverse the burn
        var reversalTransaction = OrderLoyalty.CreateEarnTransaction(
            _order.Id,
            originalTransaction.Points,
            $"Reversal of burn transaction {originalBurnTransactionId}");

        _loyaltyTransactions.Add(reversalTransaction);

        return reversalTransaction.Id;
    }

    #region Loyalty Burn Private Helpers

    private void ValidateLoyaltyBurnRequest(int points)
    {
        if (points <= 0)
        {
            throw new ArgumentException("Loyalty points must be greater than zero", nameof(points));
        }
    }

    private void ValidateOrderStateForLoyaltyBurn()
    {
        if (_order.OrderState == OrderState.Cancelled || _order.OrderState == OrderState.Refunded)
        {
            throw new InvalidOperationException("Cannot burn loyalty points for cancelled or refunded order");
        }
    }

    #endregion
}