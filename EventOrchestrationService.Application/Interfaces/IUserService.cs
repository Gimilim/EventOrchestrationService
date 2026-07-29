using EventOrchestrationService.Application.DTOs;

namespace EventOrchestrationService.Application.Interfaces;

public interface IUserService
{
    Task RegisterAsync(RegisterDataDto registerData, CancellationToken cancellationToken);
}