using EventOrchestrationService.Contracts.Enums;

namespace UserService.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(int id, string login, Role role);
}