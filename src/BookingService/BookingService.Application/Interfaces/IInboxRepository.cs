using BookingService.Domain.Entities;

namespace BookingService.Application.Interfaces;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(string eventId, string topic, CancellationToken cancellationToken = default);
    Task AddAsync(InboxMessage message, CancellationToken cancellationToken = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}