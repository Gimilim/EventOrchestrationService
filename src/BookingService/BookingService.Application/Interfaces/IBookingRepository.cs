using BookingService.Domain.Entities;

namespace BookingService.Application.Interfaces;

public interface IBookingRepository
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Booking newBooking, CancellationToken cancellationToken = default);
    Task<List<Booking>> GetPendingBookingsAsync(CancellationToken cancellationToken = default);
    Task<int> CountBookingsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<Booking>> GetPendingBookingsOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}