using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Data.Repositories;

public class OutboxRepository(AppDbContext dbContext) : IOutboxRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync(int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.Attempts < 3)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await dbContext.OutboxMessages.FindAsync([id], cancellationToken);
        message?.MarkAsProcessed();
    }

    public async Task IncrementAttemptsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await dbContext.OutboxMessages.FindAsync([id], cancellationToken);
        message?.IncrementAttempts();
    }
}