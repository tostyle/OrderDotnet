
namespace Application.DTOs;

/// <summary>
/// Response DTO for order information
/// </summary>
public record OrderDto(
    Guid Id,
    string ReferenceId,
    string OrderState,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    long Version,
    string? WorkflowId = null
);

/// <summary>
/// Request DTO for creating a new order
/// </summary>
public record CreateOrderRequest();

/// <summary>
/// Request DTO for initializing a new order
/// </summary>
public record InitialOrderRequest(
    string? ReferenceId = null,
    string PaymentMethod = "CreditCard",
    decimal PaymentAmount = 0.0m,
    string Currency = "USD"
);

/// <summary>
/// Response DTO for initial order creation
/// </summary>
public record InitialOrderResponse(
    Guid OrderId,
    string ReferenceId,
    Guid PaymentId,
    string PaymentStatus
);

/// <summary>
/// Request DTO for stock reservation
/// </summary>
public record ReserveStockRequest(
    Guid OrderId,
    Guid ProductId,
    int Quantity
);

/// <summary>
/// Response DTO for stock reservation
/// </summary>
public record ReserveStockResponse(
    Guid ReservationId,
    Guid ProductId,
    int Quantity,
    string Status
);

/// <summary>
/// Request DTO for earning loyalty points
/// </summary>
public record EarnLoyaltyRequest(
    Guid OrderId,
    int Points,
    string? Description = null
);

/// <summary>
/// Request DTO for burning loyalty points
/// </summary>
public record BurnLoyaltyRequest(
    Guid OrderId,
    int Points,
    string? Description = null
);

/// <summary>
/// Response DTO for loyalty transactions
/// </summary>
public record LoyaltyTransactionResponse(
    Guid TransactionId,
    string TransactionType,
    int Points,
    string Description,
    DateTime TransactionDate
);

/// <summary>
/// Request DTO for processing payment
/// </summary>
public record ProcessPaymentRequest(
    Guid PaymentId,
    string TransactionReference,
    string? Notes = null
);

/// <summary>
/// Response DTO for payment processing
/// </summary>
public record ProcessPaymentResponse(
    Guid PaymentId,
    string Status,
    DateTime? PaidDate,
    string? TransactionReference
);

/// <summary>
/// Response DTO for paginated order list
/// </summary>
public record OrderListResponse(
    IEnumerable<OrderDto> Orders,
    int TotalCount,
    int Skip,
    int Take
);
