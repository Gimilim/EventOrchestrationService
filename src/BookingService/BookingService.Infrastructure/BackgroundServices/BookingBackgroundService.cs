using BookingService.Application.Interfaces;
using BookingService.Infrastructure.Logging;
using EventOrchestrationService.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.BackgroundServices;

public class BookingBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingBackgroundService> logger,
    IEventPublisher eventPublisher)
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

                    try
                    {
                        await eventPublisher.PublishAsync(
                            topic: "booking-created",
                            message: evt,
                            key: booking.EventId.ToString(),
                            cancellationToken: cancellationToken
                        );

                        logger.LogBookingRetry(booking.Id, booking.RetryCount);
                    }
                    catch (Exception ex)
                    {
                        logger.LogBookingPublishError(ex, booking.Id);
                    }

                    await bookingRepository.SaveChangesAsync(cancellationToken);
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