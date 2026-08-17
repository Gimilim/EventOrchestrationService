using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Infrastructure.Data.Transactions;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Data.Repositories;

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

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new EntityFrameworkTransaction(transaction);
    }
}