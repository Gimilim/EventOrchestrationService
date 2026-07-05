using EventOrchestrationService.Data.Repositories.Interfaces;
using EventOrchestrationService.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.Data.Repositories.Implementations;

public class BookingRepository(AppDbContext dbContext): IBookingRepository
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddAsync(Booking newBooking, CancellationToken cancellationToken = default)
    {
        await dbContext.Bookings.AddAsync(newBooking, cancellationToken);
    }
}