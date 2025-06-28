using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementation of IOrderLoyaltyRepository using Entity Framework Core
/// </summary>
public class OrderLoyaltyRepository : IOrderLoyaltyRepository
{
    private readonly OrderDbContext _context;

    public OrderLoyaltyRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OrderLoyalty?> GetByIdAsync(LoyaltyTransactionId transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderLoyaltyTransactions
            .FirstOrDefaultAsync(l => l.Id == transactionId, cancellationToken);
    }

    public async Task<IEnumerable<OrderLoyalty>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderLoyaltyTransactions
            .Where(l => l.OrderId == orderId)
            .OrderByDescending(l => l.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderLoyalty>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default)
    {
        return await _context.OrderLoyaltyTransactions
            .OrderByDescending(l => l.TransactionDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OrderLoyalty loyaltyTransaction, CancellationToken cancellationToken = default)
    {
        await _context.OrderLoyaltyTransactions.AddAsync(loyaltyTransaction, cancellationToken);
    }

    public Task UpdateAsync(OrderLoyalty loyaltyTransaction, CancellationToken cancellationToken = default)
    {
        _context.OrderLoyaltyTransactions.Update(loyaltyTransaction);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(OrderLoyalty loyaltyTransaction, CancellationToken cancellationToken = default)
    {
        _context.OrderLoyaltyTransactions.Remove(loyaltyTransaction);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<OrderLoyalty>> GetByTypeAsync(LoyaltyTransactionType type, CancellationToken cancellationToken = default)
    {
        return await _context.OrderLoyaltyTransactions
            .Where(l => l.TransactionType == type)
            .OrderByDescending(l => l.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalEarnedPointsAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderLoyaltyTransactions
            .Where(l => l.OrderId == orderId && l.TransactionType == LoyaltyTransactionType.Earn)
            .SumAsync(l => l.Points, cancellationToken);
    }

    public async Task<int> GetTotalBurnedPointsAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderLoyaltyTransactions
            .Where(l => l.OrderId == orderId && l.TransactionType == LoyaltyTransactionType.Burn)
            .SumAsync(l => l.Points, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
