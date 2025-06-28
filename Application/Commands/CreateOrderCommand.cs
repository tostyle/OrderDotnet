using MediatR;
using Application.DTOs;

namespace Application.Commands;

/// <summary>
/// Command to create a new order
/// </summary>
public record CreateOrderCommand() : IRequest<OrderDto>;
