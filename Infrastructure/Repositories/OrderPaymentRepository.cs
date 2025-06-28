using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementation of IOrderPaymentRepository using Entity Framework Core
/// </summary>
public class OrderPaymentRepository : IOrderPaymentRepository
{
    private readonly OrderDbContext _context;

    public OrderPaymentRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OrderPayment?> GetByIdAsync(PaymentId paymentId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderPayments
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
    }

    public async Task<IEnumerable<OrderPayment>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderPayments
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.PaidDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderPayment>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default)
    {
        return await _context.OrderPayments
            .OrderByDescending(p => p.PaidDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OrderPayment payment, CancellationToken cancellationToken = default)
    {
        await _context.OrderPayments.AddAsync(payment, cancellationToken);
    }

    public Task UpdateAsync(OrderPayment payment, CancellationToken cancellationToken = default)
    {
        _context.OrderPayments.Update(payment);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(OrderPayment payment, CancellationToken cancellationToken = default)
    {
        _context.OrderPayments.Remove(payment);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<OrderPayment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.OrderPayments
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.PaidDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
