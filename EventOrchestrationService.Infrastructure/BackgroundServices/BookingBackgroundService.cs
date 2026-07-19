using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventOrchestrationService.Infrastructure.BackgroundServices;

public class BookingBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingBackgroundService> logger)
    : BackgroundService
{
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var pendingBookings = await bookingRepository.GetPendingBookingsAsync(cancellationToken);

                if (pendingBookings.Count != 0)
                {
                    var tasks = pendingBookings.Select(booking =>
                        ProcessBookingAsync(booking, cancellationToken));
                    await Task.WhenAll(tasks);
                }
                else
                {
                    await Task.Delay(5000, cancellationToken);
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

    private async Task ProcessBookingAsync(Booking booking, CancellationToken cancellationToken)
    {
        try
        {
            // имитация долгой обработки
            await Task.Delay(10000, cancellationToken);

            await _processingSemaphore.WaitAsync(cancellationToken);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                var targetEvent = await eventService.GetEventByIdAsync(booking.EventId, cancellationToken);
                var targetBooking = await bookingService.GetBookingByIdAsync(booking.Id, cancellationToken);

                if (targetBooking == null)
                {
                    logger.LogWarning("Бронь {BookingId} не найдена", booking.Id);
                    return;
                }

                if (targetEvent == null)
                {
                    targetBooking.Reject();
                    await bookingRepository.SaveChangesAsync(cancellationToken);

                    logger.LogWarning("Событие с ID = {EventId} не найдено, бронь с ID = {BookingId} отклонена",
                        booking.EventId, booking.Id);
                    return;
                }

                targetBooking.Confirm();
                await bookingRepository.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Обработка брони c ID {BookingId} отменена.", booking.Id);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Неожиданная ошибка при обработке брони {BookingId}", booking.Id);

            await _processingSemaphore.WaitAsync(cancellationToken);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                var targetBooking = await bookingService.GetBookingByIdAsync(booking.Id, cancellationToken);

                if (targetBooking != null)
                {
                    targetBooking.Reject();

                    var targetEvent = await eventService.GetEventByIdAsync(booking.EventId, cancellationToken);
                    if (targetEvent != null)
                    {
                        targetEvent.ReleaseSeats();
                    }

                    await bookingRepository.SaveChangesAsync(cancellationToken);

                    logger.LogInformation(
                        "Бронь с ID = {BookingId} отклонена, место возвращено событию с ID = {EventId}",
                        booking.Id, booking.EventId);
                }
            }
            catch (Exception innerEx)
            {
                logger.LogError(innerEx, "Критическая ошибка при обработке исключения для брони {BookingId}",
                    booking.Id);
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }
    }
}