using EventService.Application.Interfaces;

namespace EventService.Infrastructure.Data.UnitOfWork;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<ITransactionWrapper> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var efTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return new TransactionWrapper(efTransaction);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}