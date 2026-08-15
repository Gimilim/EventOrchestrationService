using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
using EventOrchestrationService.Contracts.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Data.Repositories;

public class BookingRepository(AppDbContext dbContext) : IBookingRepository
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException("Данные были изменены другим пользователем. Попробуйте снова.");
        }
    }

    public async Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddAsync(Booking newBooking, CancellationToken cancellationToken = default)
    {
        await dbContext.Bookings.AddAsync(newBooking, cancellationToken);
    }

    public async Task<List<Booking>> GetPendingBookingsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountBookingsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .CountAsync(
                b => b.UserId == userId && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed),
                cancellationToken);
    }

    public async Task<List<Booking>> GetPendingBookingsOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt < threshold)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}