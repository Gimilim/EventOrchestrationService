using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Domain.Entities;
using EventOrchestrationService.Domain.Enums;
using EventOrchestrationService.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.Infrastructure.Data.Repositories;

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
}