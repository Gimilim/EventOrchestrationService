namespace EventService.Application.Interfaces;

public interface ITransactionWrapper : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}