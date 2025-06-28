using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// OrderAggregate partial class containing payment processing logic
/// </summary>
public partial class OrderAggregate
{
    /// <summary>
    /// Processes a new payment for the order
    /// </summary>
    /// <param name="paymentMethod">The payment method to use</param>
    /// <param name="amount">The payment amount</param>
    /// <param name="currency">The currency (default USD)</param>
    /// <returns>The payment ID for tracking</returns>
    public PaymentId ProcessPayment(PaymentMethod paymentMethod, decimal amount, string currency = "USD")
    {
        ValidatePaymentRequest(amount);
        ValidateOrderStateForPayment();

        var payment = OrderPayment.Create(_order.Id, paymentMethod, amount, currency);
        _payments.Add(payment);

        // Check if order becomes fully paid and auto-transition
        if (ShouldTransitionToPaid())
        {
            TransitionTo(OrderState.Paid, "Payment completed - order fully paid");
        }

        return payment.Id;
    }

    /// <summary>
    /// Confirms a pending payment
    /// </summary>
    /// <param name="paymentId">The payment ID to confirm</param>
    public void ConfirmPayment(PaymentId paymentId)
    {
        var payment = GetPaymentById(paymentId);
        ValidatePaymentForConfirmation(payment);

        payment.MarkAsSuccessful("Payment confirmed by system", "Payment confirmation processed");

        // Check if order becomes fully paid after confirmation
        if (ShouldTransitionToPaid())
        {
            TransitionTo(OrderState.Paid, "All payments confirmed - order fully paid");
        }
    }

    /// <summary>
    /// Fails a pending payment
    /// </summary>
    /// <param name="paymentId">The payment ID to fail</param>
    /// <param name="reason">The reason for failure</param>
    public void FailPayment(PaymentId paymentId, string reason)
    {
        var payment = GetPaymentById(paymentId);
        ValidatePaymentForFailure(payment);

        payment.MarkAsFailed(reason);

        // If order was paid but payment failed, might need to revert state
        // This would depend on business rules
    }

    /// <summary>
    /// Refunds a successful payment
    /// </summary>
    /// <param name="paymentId">The payment ID to refund</param>
    /// <param name="refundAmount">The amount to refund (partial refunds allowed)</param>
    /// <param name="reason">The reason for refund</param>
    public void RefundPayment(PaymentId paymentId, decimal refundAmount, string reason)
    {
        var payment = GetPaymentById(paymentId);
        ValidatePaymentForRefund(payment, refundAmount);

        // Create refund record (could be a separate entity)
        payment.Refund(reason);

        // Check if should transition to refunded state
        if (ShouldTransitionToRefunded())
        {
            TransitionTo(OrderState.Refunded, reason);
        }
    }

    #region Payment Private Helpers

    private void ValidatePaymentRequest(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero", nameof(amount));
        }
    }

    private void ValidateOrderStateForPayment()
    {
        if (_order.OrderState == OrderState.Cancelled || _order.OrderState == OrderState.Refunded)
        {
            throw new InvalidOperationException("Cannot process payment for cancelled or refunded order");
        }
    }

    private OrderPayment GetPaymentById(PaymentId paymentId)
    {
        var payment = _payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment == null)
        {
            throw new InvalidOperationException($"Payment {paymentId} not found");
        }
        return payment;
    }

    private void ValidatePaymentForConfirmation(OrderPayment payment)
    {
        if (payment.Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Payment {payment.Id} is not in pending status");
        }
    }

    private void ValidatePaymentForFailure(OrderPayment payment)
    {
        if (payment.Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Payment {payment.Id} is not in pending status");
        }
    }

    private void ValidatePaymentForRefund(OrderPayment payment, decimal refundAmount)
    {
        if (payment.Status != PaymentStatus.Successful)
        {
            throw new InvalidOperationException($"Payment {payment.Id} is not successful and cannot be refunded");
        }

        if (refundAmount <= 0 || refundAmount > payment.Amount)
        {
            throw new ArgumentException("Refund amount must be positive and not exceed payment amount");
        }
    }

    private bool ShouldTransitionToPaid()
    {
        // Business logic: all payments are successful and cover the order amount
        // Note: This assumes we have order total calculation logic elsewhere
        return _payments.All(p => p.Status == PaymentStatus.Successful) &&
               _payments.Any() &&
               _order.OrderState == OrderState.Pending;
    }

    private bool ShouldTransitionToRefunded()
    {
        // Business logic: all successful payments have been refunded
        return _payments.Where(p => p.Status == PaymentStatus.Successful || p.Status == PaymentStatus.Refunded)
                       .Any(p => p.Status == PaymentStatus.Refunded);
    }

    #endregion
}