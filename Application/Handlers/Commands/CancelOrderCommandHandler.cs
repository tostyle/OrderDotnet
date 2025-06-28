using MediatR;
using Domain.Repositories;
using Application.Commands;
using Application.Extensions;

namespace Application.Handlers.Commands;

/// <summary>
/// Handler for CancelOrderCommand
/// </summary>
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        // Get order by ID
        var orderId = request.OrderId.ToOrderId();
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order == null)
        {
            return false; // Order not found
        }

        // Save changes
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
