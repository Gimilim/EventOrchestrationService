namespace EventOrchestrationService.Application.Interfaces;

public interface IUserRepository
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}