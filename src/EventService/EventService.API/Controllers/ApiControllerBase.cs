using System.Security.Claims;
using EventOrchestrationService.Contracts.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected int GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedException("ИД пользователя не найден в токене.");

        if (!int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Неверный формат ИД.");

        return userId;
    }
}