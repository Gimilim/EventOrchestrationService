using EventOrchestrationService.Domain.Enums;

namespace EventOrchestrationService.Application.DTOs;

public class RegisterDataDto
{
    public string Login { get; set; }
    public string Password { get; set; }
    public Role? Role { get; set; }
}