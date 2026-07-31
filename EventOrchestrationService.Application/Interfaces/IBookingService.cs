using EventOrchestrationService.Domain.Entities;

namespace EventOrchestrationService.Application.Interfaces;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(int eventId, int userId, CancellationToken cancellationToken);
    Task<Booking?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken);
    Task CancelBookingAsync(int bookingId, int userId, bool skipCancelPermission, CancellationToken cancellationToken);
}