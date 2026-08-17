using BookingService.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookingService.Infrastructure.Data.Transactions;

public class EntityFrameworkTransaction(IDbContextTransaction transaction) : ITransaction
{
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await transaction.RollbackAsync(cancellationToken);
    }

    public void Dispose()
    {
        transaction?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}