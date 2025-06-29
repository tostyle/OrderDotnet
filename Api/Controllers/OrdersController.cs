using Microsoft.AspNetCore.Mvc;
using Application.UseCases;
using Application.DTOs;
using Application.Services;
using Temporalio.Client;
using Workflow.Workflows;

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
    private readonly OrderService _orderService;
    private readonly ITemporalClient _temporalClient;

    public OrdersController(
        ILogger<OrdersController> logger,
        InitialOrderUseCase initialOrderUseCase,
        OrderService orderService,
        ITemporalClient temporalClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _initialOrderUseCase = initialOrderUseCase ?? throw new ArgumentNullException(nameof(initialOrderUseCase));
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _temporalClient = temporalClient ?? throw new ArgumentNullException(nameof(temporalClient));
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

            _logger.LogInformation("Successfully created initial order with OrderId: {OrderId}", response.OrderId);

            // Start OrderProcessingWorkflow for the created order
            var workflowId = $"order-processing-{response.OrderId}";
            var workflowHandle = await _temporalClient.StartWorkflowAsync(
                (OrderProcessingWorkflow wf) => wf.RunAsync(response.OrderId),
                new WorkflowOptions
                {
                    Id = workflowId,
                    TaskQueue = "order-processing"
                });

            // Associate the workflow ID with the order

            _logger.LogInformation("Started OrderProcessingWorkflow with WorkflowId: {WorkflowId} for OrderId: {OrderId}",
                workflowId, response.OrderId);

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
    public async Task<ActionResult> ProcessPayment(
        [FromRoute] Guid orderId,
        [FromBody] ProcessPaymentSignalRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing payment for order {OrderId} with payment ID: {PaymentId}",
                orderId, request.PaymentId);

            // Get order details to retrieve workflow ID
            var orderDetails = await _orderService.GetOrderDetailsAsync(orderId, cancellationToken);

            if (string.IsNullOrEmpty(orderDetails.Order.WorkflowId))
            {
                _logger.LogWarning("Order {OrderId} does not have an associated workflow", orderId);
                return BadRequest(new { error = "Order does not have an associated workflow" });
            }

            // Send payment success signal to the workflow
            var workflowHandle = _temporalClient.GetWorkflowHandle(orderDetails.Order.WorkflowId);
            await workflowHandle.SignalAsync("PaymentSuccess", new object[] { orderId });

            _logger.LogInformation("Successfully sent payment success signal to workflow {WorkflowId} for order {OrderId}",
                orderDetails.Order.WorkflowId, orderId);



            return Ok(new { message = "Payment signal sent successfully", workflowId = orderDetails.Order.WorkflowId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order {OrderId} not found", orderId);
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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{orderId:guid}/cancel")]
    public async Task<ActionResult> CancelOrder(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Cancelling order {OrderId}", orderId);

            // Get order details to retrieve workflow ID
            var orderDetails = await _orderService.GetOrderDetailsAsync(orderId, cancellationToken);

            if (string.IsNullOrEmpty(orderDetails.Order.WorkflowId))
            {
                _logger.LogWarning("Order {OrderId} does not have an associated workflow", orderId);
                return BadRequest(new { error = "Order does not have an associated workflow" });
            }

            // Send cancel order signal to the workflow
            var workflowHandle = _temporalClient.GetWorkflowHandle(orderDetails.Order.WorkflowId);
            await workflowHandle.SignalAsync("CancelOrder", new object[] { orderId });

            _logger.LogInformation("Successfully sent cancel signal to workflow {WorkflowId} for order {OrderId}",
                orderDetails.Order.WorkflowId, orderId);

            return Ok(new { message = "Cancel signal sent successfully", workflowId = orderDetails.Order.WorkflowId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order {OrderId} not found", orderId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling order {OrderId}", orderId);
            return StatusCode(500, new { error = "An internal server error occurred" });
        }
    }
}
