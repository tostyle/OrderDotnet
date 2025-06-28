
namespace Application.DTOs;

/// <summary>
/// Response DTO for order information
/// </summary>
public record OrderDto(
    Guid Id,
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
/// Response DTO for paginated order list
/// </summary>
public record OrderListResponse(
    IEnumerable<OrderDto> Orders,
    int TotalCount,
    int Skip,
    int Take
);
