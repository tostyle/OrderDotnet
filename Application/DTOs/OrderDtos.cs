using Domain.Entities;
using Domain.ValueObjects;

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
)
{
    /// <summary>
    /// Creates an OrderDto from a Domain Order entity
    /// </summary>
    /// <param name="order">The domain Order entity</param>
    /// <returns>OrderDto mapped from the domain entity</returns>
    public static OrderDto FromOrder(Domain.Entities.Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        return new OrderDto(
            Id: order.Id.Value,
            ReferenceId: order.ReferenceId,
            OrderState: order.OrderState.ToString(),
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt,
            Version: order.Version,
            WorkflowId: order.WorkflowId
        );
    }
}

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
)
{
    /// <summary>
    /// Creates a ReserveStockResponse from a Domain OrderStock entity
    /// </summary>
    /// <param name="stock">The domain OrderStock entity</param>
    /// <returns>ReserveStockResponse mapped from the domain entity</returns>
    public static ReserveStockResponse FromOrderStock(Domain.Entities.OrderStock stock)
    {
        if (stock == null)
            throw new ArgumentNullException(nameof(stock));

        return new ReserveStockResponse(
            ReservationId: stock.Id.Value,
            ProductId: stock.ProductId.Value,
            Quantity: stock.QuantityReserved,
            Status: stock.Status.ToString()
        );
    }
};

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
)
{
    /// <summary>
    /// Creates a LoyaltyTransactionResponse from a Domain OrderLoyalty entity
    /// </summary>
    /// <param name="loyalty">The domain OrderLoyalty entity</param>
    /// <returns>LoyaltyTransactionResponse mapped from the domain entity</returns>
    public static LoyaltyTransactionResponse FromOrderLoyalty(Domain.Entities.OrderLoyalty loyalty)
    {
        if (loyalty == null)
            throw new ArgumentNullException(nameof(loyalty));

        return new LoyaltyTransactionResponse(
            TransactionId: loyalty.Id.Value,
            TransactionType: loyalty.TransactionType.ToString(),
            Points: loyalty.Points,
            Description: loyalty.Description,
            TransactionDate: loyalty.TransactionDate
        );
    }
};

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
)
{
    /// <summary>
    /// Creates a ProcessPaymentResponse from a Domain OrderPayment entity
    /// </summary>
    /// <param name="payment">The domain OrderPayment entity</param>
    /// <returns>ProcessPaymentResponse mapped from the domain entity</returns>
    public static ProcessPaymentResponse FromOrderPayment(Domain.Entities.OrderPayment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        return new ProcessPaymentResponse(
            PaymentId: payment.Id.Value,
            Status: payment.Status.ToString(),
            PaidDate: payment.PaidDate,
            TransactionReference: payment.TransactionReference
        );
    }
};

/// <summary>
/// Response DTO for paginated order list
/// </summary>
public record OrderListResponse(
    IEnumerable<OrderDto> Orders,
    int TotalCount,
    int Skip,
    int Take
);

/// <summary>
/// Request DTO for starting a workflow
/// </summary>
public record StartWorkflowRequest(
    Guid OrderId,
    string WorkflowId
);

/// <summary>
/// Response DTO for workflow start operation
/// </summary>
public record StartWorkflowResponse(
    Guid OrderId,
    string WorkflowId
);

/// <summary>
/// Response DTO for workflow status inquiry
/// </summary>
public record WorkflowStatusResponse(
    Guid OrderId,
    string? WorkflowId,
    bool HasWorkflow,
    string OrderState,
    DateTime LastUpdated
);

public record OrderDetailResponse(
    OrderDto Order,
    IEnumerable<LoyaltyTransactionResponse>? LoyaltyTransactions = null,
    IEnumerable<ProcessPaymentResponse>? Payments = null,
    IEnumerable<ReserveStockResponse>? StockReservations = null
)
{
    /// <summary>
    /// Creates an OrderDetailResponse from domain entities with efficient mapping
    /// </summary>
    /// <param name="order">The domain Order entity</param>
    /// <param name="loyaltyTransactions">Collection of OrderLoyalty entities</param>
    /// <param name="payments">Collection of OrderPayment entities</param>
    /// <param name="stockReservations">Collection of OrderStock entities</param>
    /// <returns>OrderDetailResponse with all related data mapped</returns>
    public static OrderDetailResponse FromDomainEntities(
        Order order,
        IEnumerable<OrderLoyalty>? loyaltyTransactions = null,
        IEnumerable<OrderPayment>? payments = null,
        IEnumerable<OrderStock>? stockReservations = null)
    {
        return new OrderDetailResponse(
            Order: OrderDto.FromOrder(order),
            LoyaltyTransactions: loyaltyTransactions?.Select(LoyaltyTransactionResponse.FromOrderLoyalty),
            Payments: payments?.Select(ProcessPaymentResponse.FromOrderPayment),
            StockReservations: stockReservations?.Select(ReserveStockResponse.FromOrderStock)
        );
    }
};

/// <summary>
/// Request DTO for processing payment signal
/// </summary>
public record ProcessPaymentSignalRequest(
    string PaymentId
);

/// <summary>
/// Comprehensive DTO for order information including related entities
/// </summary>
public record DetailedOrderDto(
    Guid Id,
    string ReferenceId,
    string OrderState,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    long Version,
    string? WorkflowId,
    OrderPaymentDto? Payment,
    IEnumerable<LoyaltyTransactionDto> LoyaltyTransactions,
    IEnumerable<StockReservationDto> StockReservations
)
{
    /// <summary>
    /// Creates a DetailedOrderDto from a Domain Order entity with navigation properties loaded
    /// </summary>
    /// <param name="order">The domain Order entity with navigation properties loaded</param>
    /// <returns>DetailedOrderDto mapped from the domain entity</returns>
    public static DetailedOrderDto FromOrder(Domain.Entities.Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        return new DetailedOrderDto(
            Id: order.Id.Value,
            ReferenceId: order.ReferenceId,
            OrderState: order.OrderState.ToString(),
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt,
            Version: order.Version,
            WorkflowId: order.WorkflowId,
            Payment: order.Payment != null ? OrderPaymentDto.FromOrderPayment(order.Payment) : null,
            LoyaltyTransactions: order.LoyaltyTransactions?.Select(LoyaltyTransactionDto.FromOrderLoyalty) ?? Enumerable.Empty<LoyaltyTransactionDto>(),
            StockReservations: order.StockReservations?.Select(StockReservationDto.FromOrderStock) ?? Enumerable.Empty<StockReservationDto>()
        );
    }
}

/// <summary>
/// DTO for order payment information
/// </summary>
public record OrderPaymentDto(
    Guid Id,
    string PaymentMethod,
    decimal Amount,
    string Currency,
    DateTime? PaidDate,
    string Status,
    string? TransactionReference
)
{
    /// <summary>
    /// Creates an OrderPaymentDto from a Domain OrderPayment entity
    /// </summary>
    /// <param name="payment">The domain OrderPayment entity</param>
    /// <returns>OrderPaymentDto mapped from the domain entity</returns>
    public static OrderPaymentDto FromOrderPayment(Domain.Entities.OrderPayment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        return new OrderPaymentDto(
            Id: payment.Id.Value,
            PaymentMethod: payment.PaymentMethod.ToString(),
            Amount: payment.Amount,
            Currency: payment.Currency,
            PaidDate: payment.PaidDate,
            Status: payment.Status.ToString(),
            TransactionReference: payment.TransactionReference
        );
    }
}

/// <summary>
/// DTO for loyalty transaction information
/// </summary>
public record LoyaltyTransactionDto(
    Guid Id,
    string TransactionType,
    int Points,
    string Description,
    DateTime TransactionDate,
    string? ExternalTransactionId
)
{
    /// <summary>
    /// Creates a LoyaltyTransactionDto from a Domain OrderLoyalty entity
    /// </summary>
    /// <param name="loyalty">The domain OrderLoyalty entity</param>
    /// <returns>LoyaltyTransactionDto mapped from the domain entity</returns>
    public static LoyaltyTransactionDto FromOrderLoyalty(Domain.Entities.OrderLoyalty loyalty)
    {
        if (loyalty == null)
            throw new ArgumentNullException(nameof(loyalty));

        return new LoyaltyTransactionDto(
            Id: loyalty.Id.Value,
            TransactionType: loyalty.TransactionType.ToString(),
            Points: loyalty.Points,
            Description: loyalty.Description,
            TransactionDate: loyalty.TransactionDate,
            ExternalTransactionId: loyalty.ExternalTransactionId
        );
    }
}

/// <summary>
/// DTO for stock reservation information
/// </summary>
public record StockReservationDto(
    Guid Id,
    Guid ProductId,
    int QuantityReserved,
    DateTime ReservationDate,
    DateTime? ExpirationDate,
    string Status,
    string? ExternalReservationId
)
{
    /// <summary>
    /// Creates a StockReservationDto from a Domain OrderStock entity
    /// </summary>
    /// <param name="stock">The domain OrderStock entity</param>
    /// <returns>StockReservationDto mapped from the domain entity</returns>
    public static StockReservationDto FromOrderStock(Domain.Entities.OrderStock stock)
    {
        if (stock == null)
            throw new ArgumentNullException(nameof(stock));

        return new StockReservationDto(
            Id: stock.Id.Value,
            ProductId: stock.ProductId.Value,
            QuantityReserved: stock.QuantityReserved,
            ReservationDate: stock.ReservationDate,
            ExpirationDate: stock.ExpirationDate,
            Status: stock.Status.ToString(),
            ExternalReservationId: stock.ExternalReservationId
        );
    }
}
