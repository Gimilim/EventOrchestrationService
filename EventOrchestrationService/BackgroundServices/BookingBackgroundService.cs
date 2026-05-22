using EventOrchestrationService.Data;
using EventOrchestrationService.Models;
using EventOrchestrationService.Queues;

namespace EventOrchestrationService.BackgroundServices;

public class BookingBackgroundService(IBookingTaskQueue bookingQueue, IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (bookingQueue.TryDequeue(out var booking))
                {
                    await ProcessBookingAsync(booking, cancellationToken);
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
                break;
            }
        }
    }

    private async Task ProcessBookingAsync(Booking booking, CancellationToken cancellationToken)
    {
        // имитация долгой обработки
        await Task.Delay(10000, cancellationToken);

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dbBooking = await dbContext.Bookings.FindAsync([booking.Id], cancellationToken);
        if (dbBooking != null)
        {
            dbBooking.Status = BookingStatus.Confirmed;
            dbBooking.ProcessedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        };
    }
}