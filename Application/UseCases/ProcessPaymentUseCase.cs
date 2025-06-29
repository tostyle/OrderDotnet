using Application.DTOs;
using Application.Services;
using Domain.Services;
using Domain.ValueObjects;
using Domain.Repositories;

namespace Application.UseCases;

/// <summary>
/// Use case for processing payment and sending workflow signals
/// Follows clean architecture principles by orchestrating domain services
/// </summary>
public class ProcessPaymentUseCase
{
    private readonly OrderService _orderService;
    private readonly IWorkflowService _workflowService;
    private readonly IOrderPaymentRepository _paymentRepository;

    public ProcessPaymentUseCase(
        OrderService orderService,
        IWorkflowService workflowService,
        IOrderPaymentRepository paymentRepository)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
    }

    /// <summary>
    /// Executes the payment processing use case
    /// Flow: 1. Validate order exists -> 2. Validate payment exists -> 3. Process payment -> 4. Send workflow signal
    /// </summary>
    /// <param name="request">The payment processing request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The payment processing response</returns>
    public async Task<ProcessPaymentUseCaseResponse> ExecuteAsync(ProcessPaymentUseCaseRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request
        if (request.OrderId == Guid.Empty)
        {
            throw new ArgumentException("OrderId is required", nameof(request.OrderId));
        }

        if (request.PaymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId is required", nameof(request.PaymentId));
        }

        var orderId = OrderId.From(request.OrderId);
        var paymentId = PaymentId.From(request.PaymentId);

        // Get order details to ensure order exists and has workflow
        var orderDetails = await _orderService.GetOrderDetailsAsync(request.OrderId, cancellationToken);

        if (orderDetails == null)
        {
            throw new InvalidOperationException($"Order {request.OrderId} not found");
        }

        if (string.IsNullOrEmpty(orderDetails.Order.WorkflowId))
        {
            throw new InvalidOperationException($"Order {request.OrderId} does not have an associated workflow");
        }

        // Validate payment exists
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new InvalidOperationException($"Payment {request.PaymentId} not found");
        }

        // Process payment using OrderService
        var processPaymentRequest = new ProcessPaymentRequest(
            PaymentId: request.PaymentId,
            TransactionReference: request.TransactionReference ?? $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Notes: request.Notes ?? "Payment processed via API"
        );

        var paymentResponse = await _orderService.ProcessPaymentAsync(processPaymentRequest, cancellationToken);

        // Send payment success signal to workflow
        await _workflowService.SendPaymentSuccessSignalAsync(
            orderId: request.OrderId,
            paymentId: request.PaymentId,
            transactionReference: paymentResponse.TransactionReference ?? processPaymentRequest.TransactionReference,
            cancellationToken: cancellationToken
        );

        return new ProcessPaymentUseCaseResponse(
            PaymentId: paymentResponse.PaymentId,
            Status: paymentResponse.Status,
            PaidDate: paymentResponse.PaidDate,
            TransactionReference: paymentResponse.TransactionReference,
            WorkflowId: orderDetails.Order.WorkflowId
        );
    }
}

/// <summary>
/// Request DTO for ProcessPaymentUseCase
/// </summary>
public record ProcessPaymentUseCaseRequest(
    Guid OrderId,
    Guid PaymentId,
    string? TransactionReference = null,
    string? Notes = null
);

/// <summary>
/// Response DTO for ProcessPaymentUseCase with workflow information
/// </summary>
public record ProcessPaymentUseCaseResponse(
    Guid PaymentId,
    string Status,
    DateTime? PaidDate,
    string? TransactionReference,
    string? WorkflowId = null
);
