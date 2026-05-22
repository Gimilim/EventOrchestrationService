using EventOrchestrationService.Data;
using EventOrchestrationService.Exceptions;
using EventOrchestrationService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService;

public class BookingService(AppDbContext dbContext) : IBookingService
{
    public async Task<Booking> CreateBookingAsync(int eventId, CancellationToken cancellationToken)
    {
        var eventExists = await dbContext.Events.AnyAsync(
            e => e.Id == eventId,
            cancellationToken
        );

        if (!eventExists)
            throw new NotFoundException($"Событие с ID {eventId} не найдено");

        var createdBooking = new Booking
        {
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            EventId = eventId
        };

        await dbContext.Bookings.AddAsync(createdBooking, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Bookings
            .AsNoTrackingWithIdentityResolution()
            .SingleAsync (b => b.Id == createdBooking.Id, cancellationToken);
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken)
    {
        return (await dbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken));
    }
}