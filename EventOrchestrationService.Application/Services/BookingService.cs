using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Domain.Entities;
using EventOrchestrationService.Domain.Enums;
using EventOrchestrationService.Domain.Exceptions;

namespace EventOrchestrationService.Application.Services;

public class BookingService(
    IEventService eventService,
    IBookingRepository bookingRepository) : IBookingService
{
    public async Task<Booking> CreateBookingAsync(int eventId, int userId, CancellationToken cancellationToken)
    {
        var targetEvent = await eventService.GetEventByIdAsync(eventId, cancellationToken);

        if (targetEvent == null)
            throw new NotFoundException($"Событие с ID {eventId} не найдено");

        if (!targetEvent.TryReserveSeats())
            throw new NoAvailableSeatsException($"На событие с ID {eventId} нет свободных мест");

        var createdBooking = new Booking(eventId, userId, BookingStatus.Pending);

        await bookingRepository.AddAsync(createdBooking, cancellationToken);
        await bookingRepository.SaveChangesAsync(cancellationToken);

        return createdBooking;
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken)
    {
        return await bookingRepository.GetByIdAsync(bookingId, cancellationToken);
    }
}