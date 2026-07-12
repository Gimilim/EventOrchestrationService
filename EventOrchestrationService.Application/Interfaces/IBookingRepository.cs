using EventOrchestrationService.Domain.Entities;

namespace EventOrchestrationService.Application.Interfaces;

public interface IBookingRepository
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Booking newBooking, CancellationToken cancellationToken = default);
    Task<List<Booking>> GetPendingBookingsAsync(CancellationToken cancellationToken = default);
}