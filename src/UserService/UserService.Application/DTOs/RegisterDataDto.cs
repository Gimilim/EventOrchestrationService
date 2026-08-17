using EventOrchestrationService.Contracts.Enums;

namespace UserService.Application.DTOs;

public class RegisterDataDto
{
    public string Login { get; set; }
    public string Password { get; set; }
    public Role? Role { get; set; }
}