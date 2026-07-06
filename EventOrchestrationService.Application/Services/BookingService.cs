using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Entities;
using EventOrchestrationService.Exceptions;

namespace EventOrchestrationService.Application.Services;

public class BookingService(IEventService eventService, IBookingRepository bookingRepository) : IBookingService
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

            await bookingRepository.AddAsync(createdBooking, cancellationToken);
            await bookingRepository.SaveChangesAsync(cancellationToken);

            return createdBooking;
        }
        finally
        {
            _bookingLock.Release();
        }
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken)
    {
        return await bookingRepository.GetByIdAsync(bookingId, cancellationToken);
    }
}