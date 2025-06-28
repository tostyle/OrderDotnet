using MediatR;
using Domain.Repositories;
using Application.Queries;
using Application.DTOs;
using Application.Extensions;

namespace Application.Handlers.Queries;

/// <summary>
/// Handler for GetOrderByIdQuery
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var orderId = request.OrderId.ToOrderId();
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        return order?.ToDto();
    }
}
