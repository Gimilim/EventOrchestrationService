using EventOrchestrationService.Data;
using EventOrchestrationService.Exceptions;
using EventOrchestrationService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService;

public class BookingService(AppDbContext dbContext, IEventService eventService) : IBookingService
{
    private readonly SemaphoreSlim _bookingLock = new(1, 1);

    public async Task<Booking> CreateBookingAsync(int eventId, CancellationToken cancellationToken)
    {
        await _bookingLock.WaitAsync(cancellationToken);

        try
        {
            var targetEvent = await eventService.GetEventByIdAsync(eventId, cancellationToken);

            if (targetEvent == null)
                throw new NotFoundException($"Событие с ID {eventId} не найдено");

            if (!targetEvent.TryReserveSeats())
                throw new NoAvailableSeatsException($"На событие с ID {eventId} нет свободных мест");

            var createdBooking = new Booking
            {
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                EventId = eventId
            };

            await dbContext.Bookings.AddAsync(createdBooking, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return createdBooking;
        }
        finally
        {
            _bookingLock.Release();
        }
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken)
    {
        return await dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
    }
}