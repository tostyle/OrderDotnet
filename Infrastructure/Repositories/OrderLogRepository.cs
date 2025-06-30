using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Infrastructure.Data;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementation of IOrderLogRepository using Entity Framework Core
/// Handles order activity logging and audit trail operations
/// </summary>
public class OrderLogRepository : IOrderLogRepository
{
    private readonly OrderDbContext _context;

    public OrderLogRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Base IRepository implementation
    public async Task<OrderLog> GetByIdAsync(Guid id)
    {
        var result = await _context.OrderLogs
            .Include(l => l.Order)
            .FirstOrDefaultAsync(l => l.Id == OrderLogId.From(id));

        return result ?? throw new InvalidOperationException($"OrderLog with ID {id} not found.");
    }

    public async Task AddAsync(OrderLog entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _context.OrderLogs.AddAsync(entity);
    }

    public void Update(OrderLog entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.OrderLogs.Update(entity);
    }

    public void Delete(OrderLog entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.OrderLogs.Remove(entity);
    }

    public void DeleteRange(IEnumerable<OrderLog> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _context.OrderLogs.RemoveRange(entities);
    }

    public async Task<IEnumerable<OrderLog>> GetAllAsync()
    {
        return await _context.OrderLogs
            .Include(l => l.Order)
            .OrderBy(l => l.ActionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderLog>> FindAsync(
        Expression<Func<OrderLog, bool>> predicate,
        Func<IQueryable<OrderLog>, IOrderedQueryable<OrderLog>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null)
    {
        IQueryable<OrderLog> query = _context.OrderLogs;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<int> CountAsync(Expression<Func<OrderLog, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await _context.OrderLogs.CountAsync();
        }
        return await _context.OrderLogs.CountAsync(predicate);
    }

    public async Task<bool> ExistsAsync(Expression<Func<OrderLog, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _context.OrderLogs.AnyAsync(predicate);
    }

    // Domain-specific methods
    public async Task<IEnumerable<OrderLog>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderLogs
            .Include(l => l.Order)
            .Where(l => l.OrderId == orderId)
            .OrderBy(l => l.ActionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderLog>> GetByOrderIdAndActionTypeAsync(OrderId orderId, string actionType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(actionType);

        return await _context.OrderLogs
            .Include(l => l.Order)
            .Where(l => l.OrderId == orderId && l.ActionType == actionType)
            .OrderBy(l => l.ActionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderLog>> GetByOrderIdAndLevelAsync(OrderId orderId, LogLevel level, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderLogs
            .Include(l => l.Order)
            .Where(l => l.OrderId == orderId && l.Level == level)
            .OrderBy(l => l.ActionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderLog>> GetErrorsByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderLogs
            .Include(l => l.Order)
            .Where(l => l.OrderId == orderId && (l.Level == LogLevel.Error || l.ErrorMessage != null))
            .OrderByDescending(l => l.ActionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderLog>> GetByOrderIdAndDateRangeAsync(OrderId orderId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderLogs
            .Include(l => l.Order)
            .Where(l => l.OrderId == orderId && l.ActionDate >= fromDate && l.ActionDate <= toDate)
            .OrderBy(l => l.ActionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderLog>> GetByOrderIdsAsync(IEnumerable<OrderId> orderIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderIds);

        return await _context.OrderLogs
            .Include(l => l.Order)
            .Where(l => orderIds.Contains(l.OrderId))
            .OrderBy(l => l.ActionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderLog>> GetByActionTypeAsync(string actionType, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionType);

        var query = _context.OrderLogs
            .Include(l => l.Order)
            .Where(l => l.ActionType == actionType);

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.ActionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.ActionDate <= toDate.Value);
        }

        return await query
            .OrderBy(l => l.ActionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderLog>> GetByPerformedByAsync(string performedBy, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(performedBy);

        var query = _context.OrderLogs
            .Include(l => l.Order)
            .Where(l => l.PerformedBy == performedBy);

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.ActionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.ActionDate <= toDate.Value);
        }

        return await query
            .OrderBy(l => l.ActionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderLog?> GetLatestByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderLogs
            .Include(l => l.Order)
            .Where(l => l.OrderId == orderId)
            .OrderByDescending(l => l.ActionDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderLogs
            .CountAsync(l => l.OrderId == orderId, cancellationToken);
    }

    public async Task<int> CountErrorsByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderLogs
            .CountAsync(l => l.OrderId == orderId && (l.Level == LogLevel.Error || l.ErrorMessage != null), cancellationToken);
    }

    public async Task<IEnumerable<OrderLog>> GetPagedByOrderIdAsync(OrderId orderId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);
        if (page < 1) throw new ArgumentException("Page must be greater than 0", nameof(page));
        if (pageSize < 1) throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

        return await _context.OrderLogs
            .Include(l => l.Order)
            .Where(l => l.OrderId == orderId)
            .OrderByDescending(l => l.ActionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderLog>> SearchByDescriptionAsync(OrderId orderId, string searchTerm, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchTerm);

        var lowerSearchTerm = searchTerm.ToLower();

        return await _context.OrderLogs
            .Include(l => l.Order)
            .Where(l => l.OrderId == orderId &&
                       (l.Description.ToLower().Contains(lowerSearchTerm) ||
                        l.ActionType.ToLower().Contains(lowerSearchTerm) ||
                        (l.ErrorMessage != null && l.ErrorMessage.ToLower().Contains(lowerSearchTerm))))
            .OrderByDescending(l => l.ActionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetStatisticsByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderLogs
            .Where(l => l.OrderId == orderId)
            .GroupBy(l => l.ActionType)
            .Select(g => new { ActionType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ActionType, x => x.Count, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
