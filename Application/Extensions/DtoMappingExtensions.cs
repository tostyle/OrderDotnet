using Application.DTOs;
using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Extensions;

/// <summary>
/// Extension methods for mapping DTOs to Domain Entities.
/// This logic belongs in the Application layer to protect the Domain's integrity.
/// It ensures that when creating entities from DTOs, all domain rules and invariants are respected.
/// </summary>
public static class DtoMappingExtensions
{
    /// <summary>
    /// Creates a new OrderStock entity from a ReserveStockRequest DTO.
    /// Note: This requires the parent OrderId to correctly associate the stock reservation.
    /// </summary>
    public static OrderStock ToOrderStock(this ReserveStockRequest request)
    {
        return OrderStock.Create(
            orderId: OrderId.From(request.OrderId),
            productId: ProductId.From(request.ProductId),
            quantity: request.Quantity
        );
    }

    /// <summary>
    /// Creates a new OrderLoyalty entity for an "Earn" transaction.
    /// </summary>
    public static OrderLoyalty ToEarnLoyalty(this EarnLoyaltyRequest request)
    {
        return OrderLoyalty.CreateEarnTransaction(
            OrderId.From(request.OrderId),
            request.Points,
            request.Description ?? "Earned loyalty points"
        );
    }

    /// <summary>
    /// Creates a new OrderLoyalty entity for a "Burn" transaction.
    /// </summary>
    public static OrderLoyalty ToBurnLoyalty(this BurnLoyaltyRequest request)
    {
        return OrderLoyalty.CreateBurnTransaction(
            OrderId.From(request.OrderId),
            request.Points,
            request.Description ?? "Redeemed loyalty points"
        );
    }



    /// <summary>
    /// Creates a new Order entity from an InitialOrderRequest DTO.
    /// This is a simplified version that just creates the order without payment.
    /// </summary>
    public static Order ToOrder(this InitialOrderRequest request)
    {
        return Order.Create(request.ReferenceId);
    }

    /// <summary>
    /// Creates a new OrderPayment entity from a ProcessPaymentRequest DTO.
    /// Note: This requires additional context like OrderId and amount that aren't in the DTO.
    /// Consider using the aggregate methods instead for proper business logic.
    /// </summary>
    public static void ApplyToPayment(this ProcessPaymentRequest request, OrderPayment payment)
    {
        // This applies the processing logic to an existing payment
        payment.MarkAsSuccessful(request.TransactionReference, request.Notes);
    }

    /// <summary>
    /// Applies workflow assignment to an existing Order entity.
    /// Uses the domain method to ensure proper business rules are followed.
    /// </summary>
    public static void ApplyWorkflow(this StartWorkflowRequest request, Order order)
    {
        if (order.Id.Value != request.OrderId)
        {
            throw new ArgumentException("Order ID mismatch between request and entity", nameof(request));
        }

        order.SetWorkflowId(request.WorkflowId);
    }
}
