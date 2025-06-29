using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Infrastructure.Data;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementation of IOrderItemRepository using Entity Framework Core
/// </summary>
public class OrderItemRepository : IOrderItemRepository
{
    private readonly OrderDbContext _context;

    public OrderItemRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region IOrderItemRepository specific methods

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .OrderBy(oi => oi.ProductId)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderItem?> GetByIdAsync(OrderItemId orderItemId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId, cancellationToken);
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdsAsync(IEnumerable<OrderId> orderIds, CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId))
            .OrderBy(oi => oi.OrderId)
            .ThenBy(oi => oi.ProductId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderItem>> GetByProductIdAsync(ProductId productId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems
            .Where(oi => oi.ProductId == productId)
            .OrderByDescending(oi => oi.OrderId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        await _context.OrderItems.AddAsync(orderItem, cancellationToken);
    }

    public Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        _context.OrderItems.Update(orderItem);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        _context.OrderItems.Remove(orderItem);
        return Task.CompletedTask;
    }

    public async Task RemoveByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        var orderItems = await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync(cancellationToken);

        _context.OrderItems.RemoveRange(orderItems);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region IRepository<OrderItem> implementation

    public async Task<OrderItem> GetByIdAsync(Guid id)
    {
        var orderItem = await _context.OrderItems
            .FirstOrDefaultAsync(oi => oi.Id.Value == id);

        return orderItem ?? throw new InvalidOperationException($"OrderItem with ID {id} not found");
    }

    public async Task AddAsync(OrderItem entity)
    {
        await _context.OrderItems.AddAsync(entity);
    }

    public void Update(OrderItem entity)
    {
        _context.OrderItems.Update(entity);
    }

    public void Delete(OrderItem entity)
    {
        _context.OrderItems.Remove(entity);
    }

    public void DeleteRange(IEnumerable<OrderItem> entities)
    {
        _context.OrderItems.RemoveRange(entities);
    }

    public async Task<IEnumerable<OrderItem>> GetAllAsync()
    {
        return await _context.OrderItems
            .OrderBy(oi => oi.OrderId)
            .ThenBy(oi => oi.ProductId)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderItem>> FindAsync(
        Expression<Func<OrderItem, bool>> predicate,
        Func<IQueryable<OrderItem>, IOrderedQueryable<OrderItem>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null)
    {
        IQueryable<OrderItem> query = _context.OrderItems;

        if (predicate != null)
            query = query.Where(predicate);

        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
        }

        if (orderBy != null)
            query = orderBy(query);

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        return await query.ToListAsync();
    }

    public async Task<int> CountAsync(Expression<Func<OrderItem, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _context.OrderItems.CountAsync();

        return await _context.OrderItems.CountAsync(predicate);
    }

    public async Task<bool> ExistsAsync(Expression<Func<OrderItem, bool>> predicate)
    {
        return await _context.OrderItems.AnyAsync(predicate);
    }

    #endregion
}
