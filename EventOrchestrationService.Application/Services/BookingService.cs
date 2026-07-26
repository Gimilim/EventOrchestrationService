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

        var reserveResult = targetEvent.TryReserveSeats();

        switch (reserveResult)
        {
            case ReservationResult.Success:
                break;
            case ReservationResult.EventAlreadyStarted:
                throw new EventAlreadyStartedException($"Событие с ID {eventId} уже началось");
            case ReservationResult.NoAvailableSeats:
                throw new NoAvailableSeatsException($"На событие с ID {eventId} нет свободных мест");
            default:
                throw new InvalidOperationException("Неизвестная ошибка бронирования");
        }

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