using EventOrchestrationService.Entities;

namespace EventOrchestrationService.Data.Repositories.Interfaces;

public interface IBookingRepository
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Booking newBooking, CancellationToken cancellationToken = default);
}