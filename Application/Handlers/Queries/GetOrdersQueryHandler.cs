using MediatR;
using Domain.Repositories;
using Application.Queries;
using Application.DTOs;
using Application.Extensions;

namespace Application.Handlers.Queries;

/// <summary>
/// Handler for GetOrdersQuery
/// </summary>
public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, OrderListResponse>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task<OrderListResponse> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(request.Skip, request.Take, cancellationToken);
        var ordersList = orders.ToList();

        // For now, we'll use the count of returned orders as total count
        // In a real implementation, you'd have a separate count query
        var totalCount = ordersList.Count;

        return ordersList.ToOrderListResponse(totalCount, request.Skip, request.Take);
    }
}
