using Microsoft.AspNetCore.Mvc;
using Application.UseCases;
using Application.DTOs;
using Application.Services;
using static Application.UseCases.ProcessPaymentUseCase;
using static Application.UseCases.CancelOrderUseCase;

namespace Api.Controllers;

/// <summary>
/// REST API controller for order management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly InitialOrderUseCase _initialOrderUseCase;
    private readonly ProcessPaymentUseCase _processPaymentUseCase;
    private readonly CancelOrderUseCase _cancelOrderUseCase;
    private readonly OrderService _orderService;

    public OrdersController(
        ILogger<OrdersController> logger,
        InitialOrderUseCase initialOrderUseCase,
        ProcessPaymentUseCase processPaymentUseCase,
        CancelOrderUseCase cancelOrderUseCase,
        OrderService orderService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _initialOrderUseCase = initialOrderUseCase ?? throw new ArgumentNullException(nameof(initialOrderUseCase));
        _processPaymentUseCase = processPaymentUseCase ?? throw new ArgumentNullException(nameof(processPaymentUseCase));
        _cancelOrderUseCase = cancelOrderUseCase ?? throw new ArgumentNullException(nameof(cancelOrderUseCase));
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    /// <summary>
    /// Creates a new initial order
    /// </summary>
    /// <param name="request">The initial order request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created order response</returns>
    [HttpPost]
    public async Task<ActionResult<InitialOrderResponse>> CreateInitialOrder(
        [FromBody] InitialOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating initial order with ReferenceId: {ReferenceId}", request.ReferenceId);

            var response = await _initialOrderUseCase.ExecuteAsync(request, cancellationToken);

            _logger.LogInformation("Successfully created initial order with OrderId: {OrderId} and started workflow", response.OrderId);

            return CreatedAtAction(nameof(CreateInitialOrder), new { id = response.OrderId }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for initial order creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating initial order");
            return StatusCode(500, new { error = "An internal server error occurred" });
        }
    }

    /// <summary>
    /// Processes payment for an order and sends payment success signal to workflow
    /// </summary>
    /// <param name="orderId">The order ID to process payment for</param>
    /// <param name="request">The payment request containing payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{orderId:guid}/payment")]
    public async Task<ActionResult<ProcessPaymentUseCaseResponse>> ProcessPayment(
        [FromRoute] Guid orderId,
        [FromBody] ProcessPaymentSignalRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing payment for order {OrderId} with payment ID: {PaymentId}",
                orderId, request.PaymentId);

            var useCaseRequest = new ProcessPaymentUseCaseRequest(
                OrderId: orderId,
                PaymentId: Guid.Parse(request.PaymentId),
                TransactionReference: null, // Will be auto-generated
                Notes: "Payment processed via API"
            );

            var response = await _processPaymentUseCase.ExecuteAsync(useCaseRequest, cancellationToken);

            _logger.LogInformation("Successfully processed payment and sent signal to workflow {WorkflowId} for order {OrderId}",
                response.WorkflowId, orderId);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for processing payment for order {OrderId}", orderId);
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order {OrderId} not found or invalid state", orderId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing payment for order {OrderId}", orderId);
            return StatusCode(500, new { error = "An internal server error occurred" });
        }
    }

    /// <summary>
    /// Cancels an order and sends cancel signal to workflow
    /// </summary>
    /// <param name="orderId">The order ID to cancel</param>
    /// <param name="request">The cancellation request (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{orderId:guid}/cancel")]
    public async Task<ActionResult<CancelOrderUseCaseResponse>> CancelOrder(
        [FromRoute] Guid orderId,
        [FromBody] CancelOrderUseCaseRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Cancelling order {OrderId}", orderId);

            var useCaseRequest = new CancelOrderUseCaseRequest(
                OrderId: orderId,
                Reason: request?.Reason ?? "Manual cancellation via API"
            );

            var response = await _cancelOrderUseCase.ExecuteAsync(useCaseRequest, cancellationToken);

            _logger.LogInformation("Successfully sent cancel signal to workflow {WorkflowId} for order {OrderId}",
                response.WorkflowId, orderId);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for cancelling order {OrderId}", orderId);
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order {OrderId} not found or invalid state", orderId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling order {OrderId}", orderId);
            return StatusCode(500, new { error = "An internal server error occurred" });
        }
    }
}

