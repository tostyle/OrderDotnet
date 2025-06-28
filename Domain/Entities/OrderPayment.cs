using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Represents a payment transaction for an order
/// </summary>
public class OrderPayment
{
    /// <summary>
    /// Unique identifier for the payment
    /// </summary>
    public PaymentId Id { get; private set; }

    /// <summary>
    /// The order this payment belongs to
    /// </summary>
    public OrderId OrderId { get; private set; }

    /// <summary>
    /// Payment method used
    /// </summary>
    public PaymentMethod PaymentMethod { get; private set; }

    /// <summary>
    /// Amount paid
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Currency of the payment
    /// </summary>
    public string Currency { get; private set; }

    /// <summary>
    /// When the payment was processed
    /// </summary>
    public DateTime? PaidDate { get; private set; }

    /// <summary>
    /// Current status of the payment
    /// </summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>
    /// External transaction reference from payment processor
    /// </summary>
    public string? TransactionReference { get; private set; }

    /// <summary>
    /// Additional payment details or notes
    /// </summary>
    public string? Notes { get; private set; }

    // Private constructor for EF Core
    private OrderPayment()
    {
        Id = null!;
        OrderId = null!;
        PaymentMethod = null!;
        Currency = string.Empty;
    }

    /// <summary>
    /// Creates a new pending payment
    /// </summary>
    public static OrderPayment Create(
        OrderId orderId,
        PaymentMethod paymentMethod,
        decimal amount,
        string currency = "USD")
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive", nameof(amount));

        return new OrderPayment
        {
            Id = PaymentId.New(),
            OrderId = orderId,
            PaymentMethod = paymentMethod,
            Amount = amount,
            Currency = currency,
            Status = PaymentStatus.Pending
        };
    }

    /// <summary>
    /// Marks the payment as successful
    /// </summary>
    public void MarkAsSuccessful(string transactionReference, string? notes = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot mark payment as successful. Current status: {Status}");

        Status = PaymentStatus.Successful;
        PaidDate = DateTime.UtcNow;
        TransactionReference = transactionReference;
        Notes = notes;
    }

    /// <summary>
    /// Marks the payment as failed
    /// </summary>
    public void MarkAsFailed(string? notes = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot mark payment as failed. Current status: {Status}");

        Status = PaymentStatus.Failed;
        Notes = notes;
    }

    /// <summary>
    /// Refunds the payment
    /// </summary>
    public void Refund(string? notes = null)
    {
        if (Status != PaymentStatus.Successful)
            throw new InvalidOperationException($"Cannot refund payment. Current status: {Status}");

        Status = PaymentStatus.Refunded;
        Notes = notes;
    }
}

/// <summary>
/// Payment status enumeration
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending processing
    /// </summary>
    Pending,

    /// <summary>
    /// Payment was successful
    /// </summary>
    Successful,

    /// <summary>
    /// Payment failed
    /// </summary>
    Failed,

    /// <summary>
    /// Payment was refunded
    /// </summary>
    Refunded
}
