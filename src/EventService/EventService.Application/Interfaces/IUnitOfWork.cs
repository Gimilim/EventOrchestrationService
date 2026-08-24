namespace EventService.Application.Interfaces;

public interface IUnitOfWork
{
    Task<ITransactionWrapper> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}