using Application.DTOs;
using Application.Services;

namespace Application.UseCases;

/// <summary>
/// Use case for handling initial order creation
/// </summary>
public class InitialOrderUseCase
{
    private readonly OrderService _orderService;

    public InitialOrderUseCase(OrderService orderService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    /// <summary>
    /// Executes the initial order creation use case
    /// </summary>
    /// <param name="request">The initial order request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The initial order response</returns>
    public async Task<InitialOrderResponse> ExecuteAsync(InitialOrderRequest request, CancellationToken cancellationToken = default)
    {
        return await _orderService.InitialOrderAsync(request, cancellationToken);
    }
}
