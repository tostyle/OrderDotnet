using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementation of IOrderRepository using Entity Framework Core
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Order?> GetByIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Remove(order);
        return Task.CompletedTask;
    }

    public async Task<Order?> GetByWorkflowIdAsync(string workflowId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.WorkflowId == workflowId, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
