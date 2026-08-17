using EventService.Domain.Entities;

namespace EventService.Application.Interfaces;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(string eventId, string topic, CancellationToken cancellationToken = default);
    Task AddAsync(InboxMessage message, CancellationToken cancellationToken = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}