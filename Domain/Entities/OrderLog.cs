using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Entity to record any order log action for comprehensive audit trail
/// Captures all activities, events, and operations related to an order
/// </summary>
public class OrderLog
{
    /// <summary>
    /// Unique identifier for the order log record
    /// </summary>
    public OrderLogId Id { get; private set; }

    /// <summary>
    /// The order this log record belongs to
    /// </summary>
    public OrderId OrderId { get; private set; }

    /// <summary>
    /// Type of log action (e.g., "StateTransition", "PaymentProcessed", "StockReserved")
    /// </summary>
    public string ActionType { get; private set; }

    /// <summary>
    /// Detailed description of the action
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Severity level of the log entry
    /// </summary>
    public LogLevel Level { get; private set; }

    /// <summary>
    /// User or system that performed the action
    /// </summary>
    public string? PerformedBy { get; private set; }

    /// <summary>
    /// IP address or system identifier where the action originated
    /// </summary>
    public string? Source { get; private set; }

    /// <summary>
    /// Additional data related to the action (JSON format)
    /// </summary>
    public string? Data { get; private set; }

    /// <summary>
    /// Error message if the action resulted in an error
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Stack trace if an exception occurred
    /// </summary>
    public string? StackTrace { get; private set; }

    /// <summary>
    /// When the action occurred
    /// </summary>
    public DateTime ActionDate { get; private set; }

    /// <summary>
    /// When this log record was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Navigation property back to the parent Order
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    // Private constructor for EF Core
    private OrderLog()
    {
        Id = null!;
        OrderId = null!;
        ActionType = string.Empty;
        Description = string.Empty;
    }

    /// <summary>
    /// Creates a new order log record
    /// </summary>
    /// <param name="orderId">The order this log belongs to</param>
    /// <param name="actionType">Type of action being logged</param>
    /// <param name="description">Description of the action</param>
    /// <param name="level">Log level</param>
    /// <param name="performedBy">Who performed the action</param>
    /// <param name="source">Source system or IP address</param>
    /// <param name="data">Additional data in JSON format</param>
    /// <returns>New OrderLog instance</returns>
    public static OrderLog Create(
        OrderId orderId,
        string actionType,
        string description,
        LogLevel level = LogLevel.Information,
        string? performedBy = null,
        string? source = null,
        string? data = null)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        if (string.IsNullOrWhiteSpace(actionType))
            throw new ArgumentException("Action type is required", nameof(actionType));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        var now = DateTime.UtcNow;

        return new OrderLog
        {
            Id = OrderLogId.New(),
            OrderId = orderId,
            ActionType = actionType.Trim(),
            Description = description.Trim(),
            Level = level,
            PerformedBy = performedBy?.Trim(),
            Source = source?.Trim(),
            Data = data?.Trim(),
            ActionDate = now,
            CreatedAt = now
        };
    }

    /// <summary>
    /// Creates an error log record
    /// </summary>
    /// <param name="orderId">The order this log belongs to</param>
    /// <param name="actionType">Type of action that failed</param>
    /// <param name="description">Description of what was attempted</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="stackTrace">Stack trace if available</param>
    /// <param name="performedBy">Who performed the action</param>
    /// <param name="source">Source system or IP address</param>
    /// <param name="data">Additional data in JSON format</param>
    /// <returns>New OrderLog instance for error</returns>
    public static OrderLog CreateError(
        OrderId orderId,
        string actionType,
        string description,
        string errorMessage,
        string? stackTrace = null,
        string? performedBy = null,
        string? source = null,
        string? data = null)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message is required", nameof(errorMessage));

        var log = Create(orderId, actionType, description, LogLevel.Error, performedBy, source, data);
        log.ErrorMessage = errorMessage.Trim();
        log.StackTrace = stackTrace?.Trim();

        return log;
    }

    /// <summary>
    /// Creates a state transition log record
    /// </summary>
    /// <param name="orderId">The order this log belongs to</param>
    /// <param name="oldState">Previous state</param>
    /// <param name="newState">New state</param>
    /// <param name="reason">Reason for transition</param>
    /// <param name="performedBy">Who performed the transition</param>
    /// <param name="source">Source system</param>
    /// <returns>New OrderLog instance for state transition</returns>
    public static OrderLog CreateStateTransition(
        OrderId orderId,
        OrderState oldState,
        OrderState newState,
        string? reason = null,
        string? performedBy = null,
        string? source = null)
    {
        var description = $"Order state changed from {oldState} to {newState}";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            description += $" - {reason}";
        }

        var data = System.Text.Json.JsonSerializer.Serialize(new
        {
            oldState = oldState.ToString(),
            newState = newState.ToString(),
            reason = reason
        });

        return Create(
            orderId: orderId,
            actionType: "StateTransition",
            description: description,
            level: LogLevel.Information,
            performedBy: performedBy,
            source: source,
            data: data
        );
    }

    /// <summary>
    /// Creates a payment log record
    /// </summary>
    /// <param name="orderId">The order this log belongs to</param>
    /// <param name="paymentAction">Payment action (e.g., "Processed", "Failed", "Refunded")</param>
    /// <param name="amount">Payment amount</param>
    /// <param name="paymentMethod">Payment method used</param>
    /// <param name="performedBy">Who processed the payment</param>
    /// <param name="source">Source system</param>
    /// <returns>New OrderLog instance for payment action</returns>
    public static OrderLog CreatePaymentAction(
        OrderId orderId,
        string paymentAction,
        decimal amount,
        string paymentMethod,
        string? performedBy = null,
        string? source = null)
    {
        var description = $"Payment {paymentAction.ToLower()} - Amount: {amount:C}, Method: {paymentMethod}";

        var data = System.Text.Json.JsonSerializer.Serialize(new
        {
            action = paymentAction,
            amount = amount,
            paymentMethod = paymentMethod
        });

        return Create(
            orderId: orderId,
            actionType: "Payment",
            description: description,
            level: LogLevel.Information,
            performedBy: performedBy,
            source: source,
            data: data
        );
    }

    /// <summary>
    /// Updates the error information for this log record
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="stackTrace">Stack trace</param>
    public void AddError(string errorMessage, string? stackTrace = null)
    {
        ErrorMessage = errorMessage?.Trim();
        StackTrace = stackTrace?.Trim();
        Level = LogLevel.Error;
    }

    /// <summary>
    /// Updates the additional data for this log record
    /// </summary>
    /// <param name="data">Additional data in JSON format</param>
    public void UpdateData(string? data)
    {
        Data = data?.Trim();
    }

    /// <summary>
    /// Checks if this log record represents an error
    /// </summary>
    /// <returns>True if this is an error log</returns>
    public bool IsError()
    {
        return Level == LogLevel.Error || !string.IsNullOrWhiteSpace(ErrorMessage);
    }

    /// <summary>
    /// Gets the duration since this action occurred
    /// </summary>
    /// <returns>TimeSpan since the action</returns>
    public TimeSpan GetTimeSinceAction()
    {
        return DateTime.UtcNow - ActionDate;
    }

    /// <summary>
    /// Gets a formatted log message
    /// </summary>
    /// <returns>Formatted log message with timestamp and level</returns>
    public string GetFormattedMessage()
    {
        var levelName = Level.ToString().ToUpperInvariant();
        var timestamp = ActionDate.ToString("yyyy-MM-dd HH:mm:ss");
        var message = $"[{timestamp}] [{levelName}] {ActionType}: {Description}";

        if (!string.IsNullOrWhiteSpace(PerformedBy))
        {
            message += $" (by {PerformedBy})";
        }

        return message;
    }
}

/// <summary>
/// Log level enumeration for order logs
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}
