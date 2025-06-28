using MediatR;
using Application.DTOs;

namespace Application.Queries;

/// <summary>
/// Query to get an order by its ID
/// </summary>
public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;
