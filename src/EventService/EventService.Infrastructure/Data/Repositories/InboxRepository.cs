using EventService.Application.Interfaces;
using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.Data.Repositories;

public class InboxRepository(AppDbContext dbContext) : IInboxRepository
{
    public async Task<bool> ExistsAsync(string eventId, string topic, CancellationToken cancellationToken = default)
    {
        return await dbContext.InboxMessages
            .AnyAsync(m => m.EventId == eventId && m.Topic == topic, cancellationToken);
    }

    public async Task AddAsync(InboxMessage message, CancellationToken cancellationToken = default)
    {
        await dbContext.InboxMessages.AddAsync(message, cancellationToken);
    }
}