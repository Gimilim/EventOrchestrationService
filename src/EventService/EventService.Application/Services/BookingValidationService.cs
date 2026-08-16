using EventOrchestrationService.Contracts.Events;
using EventService.Application.Interfaces;
using EventService.Domain.Enums;
using Microsoft.Extensions.Logging;
using EventService.Infrastructure.Logging;

namespace EventService.Application.Services;

public class BookingValidationService(
    IEventRepository eventRepository,
    IEventPublisher eventPublisher,
    ILogger<BookingValidationService> logger) : IBookingValidationService
{
    public async Task ValidateBookingAsync(BookingCreatedEvent evt, CancellationToken cancellationToken)
    {
        var targetEvent = await eventRepository.GetByIdAsync(evt.EventId, cancellationToken);

        if (targetEvent == null)
        {
            await PublishRejectedAsync(evt, $"Событие с ID {evt.EventId} не найдено", cancellationToken);
            logger.LogEventNotFound(evt.EventId, evt.BookingId);
            return;
        }

        var reserveResult = targetEvent.TryReserveSeats();

        if (reserveResult == ReservationResult.Success)
        {
            await eventRepository.SaveChangesAsync(cancellationToken);

            await eventPublisher.PublishAsync(
                "booking-confirmed",
                new BookingConfirmedEvent { BookingId = evt.BookingId },
                key: evt.EventId.ToString(),
                cancellationToken: cancellationToken
            );
            logger.LogBookingConfirmed(evt.BookingId, evt.EventId);
        }
        else
        {
            var reason = reserveResult switch
            {
                ReservationResult.EventAlreadyStarted => $"Событие с ID {evt.EventId} уже началось",
                ReservationResult.NoAvailableSeats => $"На событие с ID {evt.EventId} нет свободных мест",
                _ => "Неизвестная ошибка"
            };

            await PublishRejectedAsync(evt, reason, cancellationToken);
            logger.LogReservationFailed(evt.BookingId, reason);
        }
    }

    private async Task PublishRejectedAsync(BookingCreatedEvent evt, string reason, CancellationToken cancellationToken)
    {
        await eventPublisher.PublishAsync(
            "booking-rejected",
            new BookingRejectedEvent
            {
                BookingId = evt.BookingId,
                Reason = reason
            },
            key: evt.EventId.ToString(),
            cancellationToken: cancellationToken
        );
    }

    public async Task HandleBookingCancelledAsync(BookingCancelledEvent evt, CancellationToken cancellationToken)
    {
        var targetEvent = await eventRepository.GetByIdAsync(evt.EventId, cancellationToken);

        if (targetEvent == null)
        {
            logger.LogEventNotFound(evt.EventId, evt.BookingId);
            return;
        }

        targetEvent.ReleaseSeats();
        await eventRepository.SaveChangesAsync(cancellationToken);

        logger.LogSeatsReleased(evt.EventId, evt.BookingId);
    }
}