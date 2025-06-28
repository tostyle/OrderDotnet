using Domain.Entities;

namespace Domain.Aggregates;

/// <summary>
/// OrderAggregate partial class containing advanced order state transition logic
/// </summary>
public partial class OrderAggregate
{
    /// <summary>
    /// Cancels the order and releases all resources
    /// </summary>
    /// <param name="reason">Reason for cancellation</param>
    public void CancelOrder(string reason)
    {
        ValidateOrderCanBeCancelled();

        // Release all stock reservations
        ReleaseAllStockReservations($"Order cancelled: {reason}");

        // Handle payments - refund successful payments
        RefundAllSuccessfulPayments($"Order cancelled: {reason}");

        // Transition to cancelled state
        TransitionTo(OrderState.Cancelled, reason);
    }

    /// <summary>
    /// Completes the order (when delivered/fulfilled)
    /// </summary>
    /// <param name="completionNotes">Notes about order completion</param>
    public void CompleteOrder(string? completionNotes = null)
    {
        ValidateOrderCanBeCompleted();

        // Ensure all stock is fulfilled
        FulfillAllConfirmedStock();

        // Transition to completed state
        TransitionTo(OrderState.Completed, completionNotes ?? "Order completed successfully");
    }

    /// <summary>
    /// Processes a full refund for the order
    /// </summary>
    /// <param name="refundReason">Reason for the refund</param>
    public void ProcessFullRefund(string refundReason)
    {
        ValidateOrderCanBeRefunded();

        // Refund all successful payments
        RefundAllSuccessfulPayments(refundReason);

        // Reverse any loyalty points that were earned
        ReverseAllLoyaltyEarnTransactions();

        // Release any remaining stock
        ReleaseAllStockReservations($"Order refunded: {refundReason}");

        // Transition to refunded state
        TransitionTo(OrderState.Refunded, refundReason);
    }

    /// <summary>
    /// Automatically processes order based on current state and conditions
    /// </summary>
    /// <param name="orderAmount">Total order amount for payment validation</param>
    public void AutoProcessOrder(decimal orderAmount)
    {
        switch (_order.OrderState)
        {
            case OrderState.Pending:
                // Check if fully paid
                if (IsFullyPaid(orderAmount))
                {
                    TransitionTo(OrderState.Paid, "Auto-transition: Order fully paid");
                }
                break;

            case OrderState.Paid:
                // Check if all stock is confirmed
                if (AllStockConfirmed())
                {
                    TransitionTo(OrderState.Completed, "Auto-transition: All stock confirmed and allocated");
                }
                break;

            default:
                // No auto-processing for other states
                break;
        }
    }

    /// <summary>
    /// Forces a state transition (use with caution - bypasses validation)
    /// </summary>
    /// <param name="newState">The target state</param>
    /// <param name="reason">Reason for forced transition</param>
    /// <param name="adminOverride">Admin identifier authorizing the override</param>
    public void ForceStateTransition(OrderState newState, string reason, string adminOverride)
    {
        if (string.IsNullOrWhiteSpace(adminOverride))
        {
            throw new ArgumentException("Admin override identifier is required for forced transitions");
        }

        var previousState = _order.OrderState;
        _order.OrderState = newState;
        _order.UpdatedAt = DateTime.UtcNow;
        _order.Version++;

        // Log the forced transition with admin override
        // TODO: Raise domain event for forced transition
        // RaiseDomainEvent(new OrderStateForcedEvent(_order.Id, previousState, newState, reason, adminOverride));
    }

    #region Transition Private Helpers

    private void ValidateOrderCanBeCancelled()
    {
        if (_order.OrderState == OrderState.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed order");
        }

        if (_order.OrderState == OrderState.Cancelled)
        {
            throw new InvalidOperationException("Order is already cancelled");
        }
    }

    private void ValidateOrderCanBeCompleted()
    {
        if (_order.OrderState != OrderState.Paid)
        {
            throw new InvalidOperationException("Order must be paid before it can be completed");
        }

        if (!HasSufficientStockReserved())
        {
            throw new InvalidOperationException("Order cannot be completed without sufficient stock reserved");
        }
    }

    private void ValidateOrderCanBeRefunded()
    {
        if (_order.OrderState == OrderState.Cancelled)
        {
            throw new InvalidOperationException("Cannot refund a cancelled order");
        }

        if (_order.OrderState == OrderState.Refunded)
        {
            throw new InvalidOperationException("Order is already refunded");
        }
    }

    private void RefundAllSuccessfulPayments(string reason)
    {
        var successfulPayments = _payments.Where(p => p.Status == PaymentStatus.Successful).ToList();

        foreach (var payment in successfulPayments)
        {
            payment.Refund(reason);
        }
    }

    private void FulfillAllConfirmedStock()
    {
        var confirmedReservations = _stockReservations
            .Where(r => r.Status == ReservationStatus.Confirmed)
            .ToList();

        foreach (var reservation in confirmedReservations)
        {
            reservation.Fulfill(null, "Stock fulfilled for order completion");
        }
    }

    private void ReverseAllLoyaltyEarnTransactions()
    {
        var earnTransactions = _loyaltyTransactions
            .Where(t => t.TransactionType == LoyaltyTransactionType.Earn)
            .ToList();

        foreach (var earnTransaction in earnTransactions)
        {
            // Create a burn transaction to reverse the earn
            var reversalTransaction = OrderLoyalty.CreateBurnTransaction(
                _order.Id,
                earnTransaction.Points,
                $"Reversal of earn transaction due to order refund");

            _loyaltyTransactions.Add(reversalTransaction);
        }
    }

    #endregion
}
