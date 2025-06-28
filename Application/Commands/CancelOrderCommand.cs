using MediatR;

namespace Application.Commands;

/// <summary>
/// Command to cancel an order
/// </summary>
public record CancelOrderCommand(Guid OrderId) : IRequest<bool>;
