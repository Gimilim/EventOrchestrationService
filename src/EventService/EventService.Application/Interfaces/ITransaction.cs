namespace EventService.Application.Interfaces;

public interface ITransaction : IAsyncDisposable, IDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}