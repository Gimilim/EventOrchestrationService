using EventOrchestrationService.Domain.Enums;

namespace EventOrchestrationService.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(int id, string login, Role role);
}