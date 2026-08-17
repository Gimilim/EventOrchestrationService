using System.Text.Json;
using EventOrchestrationService.Contracts.Events;
using EventService.Application.Interfaces;
using EventService.Application.Logging;
using EventService.Domain.Entities;
using EventService.Domain.Enums;
using Microsoft.Extensions.Logging;
using EventService.Infrastructure.Logging;
using EventLogs = EventService.Application.Logging.EventLogs;

namespace EventService.Application.Services;

public class BookingValidationService(
    IEventRepository eventRepository,
    IEventPublisher eventPublisher,
    ILogger<BookingValidationService> logger,
    IInboxRepository inboxRepository) : IBookingValidationService
{
    public async Task ValidateBookingAsync(BookingCreatedEvent evt, CancellationToken cancellationToken)
    {
        var eventId = $"{evt.BookingId}_{evt.EventId}";
        if (await inboxRepository.ExistsAsync(eventId, "booking-created", cancellationToken))
        {
            EventLogs.LogEventAlreadyProcessed((ILogger)logger, eventId);
            return;
        }

        var targetEvent = await eventRepository.GetByIdAsync(evt.EventId, cancellationToken);

        if (targetEvent == null)
        {
            await PublishRejectedAsync(evt, $"Событие с ID {evt.EventId} не найдено", cancellationToken);
            EventLogs.LogEventNotFound((ILogger)logger, evt.EventId, evt.BookingId);
            return;
        }

        var reserveResult = targetEvent.TryReserveSeats();

        if (reserveResult == ReservationResult.Success)
        {
            await using var transaction = await eventRepository.BeginTransactionAsync(cancellationToken);

            try
            {
                await eventRepository.SaveChangesAsync(cancellationToken);

                var inboxMessage = new InboxMessage(eventId, "booking-created", JsonSerializer.Serialize(evt));

                await inboxRepository.AddAsync(inboxMessage, cancellationToken);
                await eventRepository.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                await eventPublisher.PublishAsync(
                    "booking-confirmed",
                    new BookingConfirmedEvent { BookingId = evt.BookingId },
                    key: evt.EventId.ToString(),
                    cancellationToken: cancellationToken
                );

                EventLogs.LogBookingConfirmed((ILogger)logger, evt.BookingId, evt.EventId);
                EventLogs.LogEventSavedToInbox((ILogger)logger, eventId);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
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
            EventLogs.LogReservationFailed((ILogger)logger, evt.BookingId, reason);
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
            EventLogs.LogEventNotFound((ILogger)logger, evt.EventId, evt.BookingId);
            return;
        }

        await using var transaction = await eventRepository.BeginTransactionAsync(cancellationToken);

        try
        {
            targetEvent.ReleaseSeats();
            await eventRepository.SaveChangesAsync(cancellationToken);

            var eventId = $"cancelled_{evt.BookingId}_{evt.EventId}";
            var inboxMessage = new InboxMessage(eventId, "booking-cancelled", JsonSerializer.Serialize(evt));
            await inboxRepository.AddAsync(inboxMessage, cancellationToken);
            await eventRepository.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            EventLogs.LogSeatsReleased((ILogger)logger, evt.EventId, evt.BookingId);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}