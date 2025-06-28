using MediatR;
using Domain.Entities;
using Domain.Repositories;
using Application.Commands;
using Application.DTOs;
using Application.Extensions;

namespace Application.Handlers.Commands;

/// <summary>
/// Handler for CreateOrderCommand
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Create new order in PENDING state
        var order = Order.Create();

        // Save order to repository
        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return order.ToDto();
    }
}
