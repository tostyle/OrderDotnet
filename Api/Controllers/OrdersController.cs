using Microsoft.AspNetCore.Mvc;

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

    public OrdersController(ILogger<OrdersController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // All endpoints removed - controller is now empty
}
