using System.Text.Json;
using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Infrastructure.Logging;
using EventOrchestrationService.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.BackgroundServices;

public class BookingBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingBackgroundService> logger,
    IOutboxRepository outboxRepository)
    : BackgroundService
{
    private const int MaxRetryCount = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var threshold = DateTime.UtcNow.Subtract(_retryDelay);
                var pendingBookings =
                    await bookingRepository.GetPendingBookingsOlderThanAsync(threshold, cancellationToken);

                if (pendingBookings.Count == 0)
                {
                    await Task.Delay(5000, cancellationToken);
                    continue;
                }

                foreach (var booking in pendingBookings)
                {
                    if (booking.RetryCount >= MaxRetryCount)
                    {
                        booking.MarkAsFailed();
                        await bookingRepository.SaveChangesAsync(cancellationToken);
                        logger.LogFailedBooking(booking.Id, booking.RetryCount);
                        continue;
                    }

                    booking.IncrementRetryCount();

                    var evt = new BookingCreatedEvent
                    {
                        BookingId = booking.Id,
                        EventId = booking.EventId,
                        UserId = booking.UserId,
                        CreatedAt = booking.CreatedAt
                    };

                    var outboxMessage = new OutboxMessage(
                        topic: "booking-created",
                        payload: JsonSerializer.Serialize(evt),
                        key: booking.EventId.ToString()
                    );

                    await outboxRepository.AddAsync(outboxMessage, cancellationToken);
                    await bookingRepository.SaveChangesAsync(cancellationToken);

                    logger.LogBookingRetry(booking.Id, booking.RetryCount);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка в BookingBackgroundService");
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}