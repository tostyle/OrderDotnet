using MediatR;
using Application.DTOs;

namespace Application.Queries;

/// <summary>
/// Query to get all orders with pagination
/// </summary>
public record GetOrdersQuery(
    int Skip = 0,
    int Take = 100
) : IRequest<OrderListResponse>;
