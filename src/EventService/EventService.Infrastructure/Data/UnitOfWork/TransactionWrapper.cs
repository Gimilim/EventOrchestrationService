using EventService.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventService.Infrastructure.Data.UnitOfWork;

public class TransactionWrapper(IDbContextTransaction efTransaction) : ITransactionWrapper
{
    private readonly IDbContextTransaction _efTransaction = efTransaction ?? throw new ArgumentNullException(nameof(efTransaction));
    private bool _isCommitted;

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _efTransaction.CommitAsync(cancellationToken);
        _isCommitted = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isCommitted)
        {
            await _efTransaction.RollbackAsync();
        }
        await _efTransaction.DisposeAsync();
    }
}
