using EventOrchestrationService.Entities;

namespace EventOrchestrationService.Services.Interfaces;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(int eventId, CancellationToken cancellationToken);
    Task<Booking?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken);
}