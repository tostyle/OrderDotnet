using MediatR;
using FluentValidation;
using Application.Commands;
using Application.Queries;
using Application.DTOs;

namespace Application.Services;

/// <summary>
/// Application service for orchestrating order operations
/// </summary>
public interface IOrderApplicationService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderListResponse> GetOrdersAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default);
    Task<bool> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of order application service
/// </summary>
public class OrderApplicationService : IOrderApplicationService
{
    private readonly IMediator _mediator;
    private readonly IValidator<CancelOrderCommand> _cancelOrderValidator;
    private readonly IValidator<GetOrderByIdQuery> _getOrderByIdValidator;
    private readonly IValidator<GetOrdersQuery> _getOrdersValidator;

    public OrderApplicationService(
        IMediator mediator,
        IValidator<CancelOrderCommand> cancelOrderValidator,
        IValidator<GetOrderByIdQuery> getOrderByIdValidator,
        IValidator<GetOrdersQuery> getOrdersValidator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _cancelOrderValidator = cancelOrderValidator ?? throw new ArgumentNullException(nameof(cancelOrderValidator));
        _getOrderByIdValidator = getOrderByIdValidator ?? throw new ArgumentNullException(nameof(getOrderByIdValidator));
        _getOrdersValidator = getOrdersValidator ?? throw new ArgumentNullException(nameof(getOrdersValidator));
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var command = new CreateOrderCommand();
        return await _mediator.Send(command, cancellationToken);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var query = new GetOrderByIdQuery(orderId);

        // Validate query
        await _getOrderByIdValidator.ValidateAndThrowAsync(query, cancellationToken);

        return await _mediator.Send(query, cancellationToken);
    }

    public async Task<OrderListResponse> GetOrdersAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersQuery(skip, take);

        // Validate query
        await _getOrdersValidator.ValidateAndThrowAsync(query, cancellationToken);

        return await _mediator.Send(query, cancellationToken);
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var command = new CancelOrderCommand(orderId);

        // Validate command
        await _cancelOrderValidator.ValidateAndThrowAsync(command, cancellationToken);

        return await _mediator.Send(command, cancellationToken);
    }
}
