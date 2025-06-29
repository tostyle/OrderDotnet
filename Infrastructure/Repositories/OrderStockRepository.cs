using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementation of IOrderStockRepository using Entity Framework Core
/// </summary>
public class OrderStockRepository : IOrderStockRepository
{
    private readonly OrderDbContext _context;

    public OrderStockRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OrderStock?> GetByIdAsync(StockReservationId reservationId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderStockReservations
            .FirstOrDefaultAsync(s => s.Id == reservationId, cancellationToken);
    }

    public async Task<IEnumerable<OrderStock>> GetByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderStockReservations
            .Where(s => s.OrderId == orderId)
            .OrderByDescending(s => s.ReservationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderStock>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default)
    {
        return await _context.OrderStockReservations
            .OrderByDescending(s => s.ReservationDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OrderStock stockReservation, CancellationToken cancellationToken = default)
    {
        await _context.OrderStockReservations.AddAsync(stockReservation, cancellationToken);
    }

    public Task UpdateAsync(OrderStock stockReservation, CancellationToken cancellationToken = default)
    {
        _context.OrderStockReservations.Update(stockReservation);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(OrderStock stockReservation, CancellationToken cancellationToken = default)
    {
        _context.OrderStockReservations.Remove(stockReservation);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<OrderStock>> GetByStatusAsync(ReservationStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.OrderStockReservations
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.ReservationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderStock>> GetByProductIdAsync(ProductId productId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderStockReservations
            .Where(s => s.ProductId == productId)
            .OrderByDescending(s => s.ReservationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderStock?> GetByOrderIdAndProductIdAsync(OrderId orderId, ProductId productId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderStockReservations
            .Where(s => s.OrderId == orderId && s.ProductId == productId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetTotalReservedQuantityAsync(ProductId productId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderStockReservations
            .Where(s => s.ProductId == productId &&
                       (s.Status == ReservationStatus.Reserved || s.Status == ReservationStatus.Confirmed))
            .SumAsync(s => s.QuantityReserved, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
