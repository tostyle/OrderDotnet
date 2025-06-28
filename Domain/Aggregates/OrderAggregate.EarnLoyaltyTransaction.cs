using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// OrderAggregate partial class containing loyalty earning logic
/// </summary>
public partial class OrderAggregate
{
    /// <summary>
    /// Records loyalty points earned for this order
    /// </summary>
    /// <param name="points">Number of points to earn</param>
    /// <param name="description">Description of the earning transaction</param>
    /// <param name="externalTransactionId">External system transaction reference</param>
    /// <returns>The loyalty transaction ID</returns>
    public LoyaltyTransactionId EarnLoyaltyPoints(int points, string? description = null, string? externalTransactionId = null)
    {
        ValidateLoyaltyEarnRequest(points);
        ValidateOrderStateForLoyaltyEarn();

        var loyaltyTransaction = OrderLoyalty.CreateEarnTransaction(
            _order.Id,
            points,
            description ?? $"Points earned for order {_order.Id}",
            externalTransactionId);

        _loyaltyTransactions.Add(loyaltyTransaction);

        return loyaltyTransaction.Id;
    }

    /// <summary>
    /// Processes bulk loyalty point earning (for promotions, bonuses, etc.)
    /// </summary>
    /// <param name="pointsAndDescriptions">List of points and descriptions to earn</param>
    public void EarnBulkLoyaltyPoints(IEnumerable<(int Points, string Description)> pointsAndDescriptions)
    {
        ValidateOrderStateForLoyaltyEarn();

        foreach (var (points, description) in pointsAndDescriptions)
        {
            ValidateLoyaltyEarnRequest(points);

            var loyaltyTransaction = OrderLoyalty.CreateEarnTransaction(
                _order.Id,
                points,
                description);

            _loyaltyTransactions.Add(loyaltyTransaction);
        }
    }

    /// <summary>
    /// Calculates loyalty points that should be earned based on order value
    /// </summary>
    /// <param name="orderAmount">Total order amount</param>
    /// <param name="loyaltyRate">Points per currency unit (e.g., 1 point per dollar)</param>
    /// <returns>Number of points to be earned</returns>
    public int CalculateLoyaltyPointsToEarn(decimal orderAmount, decimal loyaltyRate = 1.0m)
    {
        if (orderAmount <= 0)
            throw new ArgumentException("Order amount must be positive", nameof(orderAmount));

        if (loyaltyRate <= 0)
            throw new ArgumentException("Loyalty rate must be positive", nameof(loyaltyRate));

        return (int)Math.Floor(orderAmount * loyaltyRate);
    }

    /// <summary>
    /// Automatically earns loyalty points based on order completion
    /// </summary>
    /// <param name="orderAmount">Total order amount</param>
    /// <param name="loyaltyRate">Points per currency unit</param>
    public void AutoEarnLoyaltyPoints(decimal orderAmount, decimal loyaltyRate = 1.0m)
    {
        var pointsToEarn = CalculateLoyaltyPointsToEarn(orderAmount, loyaltyRate);

        if (pointsToEarn > 0)
        {
            EarnLoyaltyPoints(pointsToEarn, $"Automatic points earned for completed order (Rate: {loyaltyRate:F2} pts/$)");
        }
    }

    #region Loyalty Earn Private Helpers

    private void ValidateLoyaltyEarnRequest(int points)
    {
        if (points <= 0)
        {
            throw new ArgumentException("Loyalty points must be greater than zero", nameof(points));
        }
    }

    private void ValidateOrderStateForLoyaltyEarn()
    {
        if (_order.OrderState != OrderState.Completed)
        {
            throw new InvalidOperationException("Can only earn loyalty points for completed orders");
        }
    }

    #endregion
}