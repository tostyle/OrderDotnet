using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Infrastructure.Data;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementation of IOrderJourneyRepository using Entity Framework Core
/// Handles order state transition audit trail operations
/// </summary>
public class OrderJourneyRepository : IOrderJourneyRepository
{
    private readonly OrderDbContext _context;

    public OrderJourneyRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Base IRepository implementation
    public async Task<OrderJourney> GetByIdAsync(Guid id)
    {
        var result = await _context.OrderJourneys
            .Include(j => j.Order)
            .FirstOrDefaultAsync(j => j.Id == OrderJourneyId.From(id));

        return result ?? throw new InvalidOperationException($"OrderJourney with ID {id} not found.");
    }

    public async Task AddAsync(OrderJourney entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _context.OrderJourneys.AddAsync(entity);
    }

    public void Update(OrderJourney entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.OrderJourneys.Update(entity);
    }

    public void Delete(OrderJourney entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.OrderJourneys.Remove(entity);
    }

    public void DeleteRange(IEnumerable<OrderJourney> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _context.OrderJourneys.RemoveRange(entities);
    }

    public async Task<IEnumerable<OrderJourney>> GetAllAsync()
    {
        return await _context.OrderJourneys
            .Include(j => j.Order)
            .OrderBy(j => j.TransitionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderJourney>> FindAsync(
        Expression<Func<OrderJourney, bool>> predicate,
        Func<IQueryable<OrderJourney>, IOrderedQueryable<OrderJourney>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null)
    {
        IQueryable<OrderJourney> query = _context.OrderJourneys;

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

    public async Task<int> CountAsync(Expression<Func<OrderJourney, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await _context.OrderJourneys.CountAsync();
        }
        return await _context.OrderJourneys.CountAsync(predicate);
    }

    public async Task<bool> ExistsAsync(Expression<Func<OrderJourney, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _context.OrderJourneys.AnyAsync(predicate);
    }

    // Domain-specific methods
    public async Task<IEnumerable<OrderJourney>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderJourneys
            .Include(j => j.Order)
            .Where(j => j.OrderId == orderId)
            .OrderBy(j => j.TransitionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderJourney>> GetByOrderIdAndStateAsync(OrderId orderId, OrderState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderJourneys
            .Include(j => j.Order)
            .Where(j => j.OrderId == orderId && (j.OldState == state || j.NewState == state))
            .OrderBy(j => j.TransitionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderJourney>> GetByOrderIdAndOldStateAsync(OrderId orderId, OrderState oldState, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderJourneys
            .Include(j => j.Order)
            .Where(j => j.OrderId == orderId && j.OldState == oldState)
            .OrderBy(j => j.TransitionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderJourney>> GetByOrderIdAndNewStateAsync(OrderId orderId, OrderState newState, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderJourneys
            .Include(j => j.Order)
            .Where(j => j.OrderId == orderId && j.NewState == newState)
            .OrderBy(j => j.TransitionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderJourney?> GetLatestByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderJourneys
            .Include(j => j.Order)
            .Where(j => j.OrderId == orderId)
            .OrderByDescending(j => j.TransitionDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderJourney>> GetByOrderIdAndDateRangeAsync(OrderId orderId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderJourneys
            .Include(j => j.Order)
            .Where(j => j.OrderId == orderId && j.TransitionDate >= fromDate && j.TransitionDate <= toDate)
            .OrderBy(j => j.TransitionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderJourney>> GetByOrderIdsAsync(IEnumerable<OrderId> orderIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderIds);

        return await _context.OrderJourneys
            .Include(j => j.Order)
            .Where(j => orderIds.Contains(j.OrderId))
            .OrderBy(j => j.TransitionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderJourney>> GetByTransitionPatternAsync(OrderState oldState, OrderState newState, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.OrderJourneys
            .Include(j => j.Order)
            .Where(j => j.OldState == oldState && j.NewState == newState);

        if (fromDate.HasValue)
        {
            query = query.Where(j => j.TransitionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(j => j.TransitionDate <= toDate.Value);
        }

        return await query
            .OrderBy(j => j.TransitionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderJourneys
            .CountAsync(j => j.OrderId == orderId, cancellationToken);
    }

    public async Task<bool> HasTransitionAsync(OrderId orderId, OrderState oldState, OrderState newState, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderJourneys
            .AnyAsync(j => j.OrderId == orderId && j.OldState == oldState && j.NewState == newState, cancellationToken);
    }

    public async Task<IEnumerable<OrderJourney>> GetStateTimelineAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        return await _context.OrderJourneys
            .Include(j => j.Order)
            .Where(j => j.OrderId == orderId)
            .OrderBy(j => j.TransitionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
