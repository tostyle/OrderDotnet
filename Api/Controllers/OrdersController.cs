using Microsoft.AspNetCore.Mvc;
using Application.UseCases;
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
    private readonly ILogger<OrdersController> _logger;
    private readonly InitialOrderUseCase _initialOrderUseCase;

    public OrdersController(ILogger<OrdersController> logger, InitialOrderUseCase initialOrderUseCase)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _initialOrderUseCase = initialOrderUseCase ?? throw new ArgumentNullException(nameof(initialOrderUseCase));
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
}
