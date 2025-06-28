using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs;

namespace Api.Controllers;

/// <summary>
/// REST API controller for order management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderApplicationService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderApplicationService orderService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created order</returns>
    [HttpPost]
    [ProducesResponseType<OrderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new order");

            var order = await _orderService.CreateOrderAsync(request, cancellationToken);

            _logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.Id },
                order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the order");
        }
    }

    /// <summary>
    /// Get an order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> GetOrderById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting order with ID: {OrderId}", id);

            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order not found with ID: {OrderId}", id);
                return NotFound($"Order with ID {id} not found");
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order with ID: {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order");
        }
    }

    /// <summary>
    /// Get all orders with pagination
    /// </summary>
    /// <param name="skip">Number of orders to skip</param>
    /// <param name="take">Number of orders to take</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of orders</returns>
    [HttpGet]
    [ProducesResponseType<OrderListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderListResponse>> GetOrders(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting orders with skip: {Skip}, take: {Take}", skip, take);

            var orders = await _orderService.GetOrdersAsync(skip, take, cancellationToken);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving orders");
        }
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancellation result</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CancelOrder(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Cancelling order with ID: {OrderId}", id);

            var result = await _orderService.CancelOrderAsync(id, cancellationToken);

            if (!result)
            {
                _logger.LogWarning("Order not found for cancellation with ID: {OrderId}", id);
                return NotFound($"Order with ID {id} not found");
            }

            _logger.LogInformation("Order cancelled successfully with ID: {OrderId}", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel order with ID: {OrderId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order with ID: {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while cancelling the order");
        }
    }
}
