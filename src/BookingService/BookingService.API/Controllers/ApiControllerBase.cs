using EventOrchestrationService.Contracts.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected int GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedException("ИД пользователя не найден в токене.");

        if (!int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Неверный формат ИД.");

        return userId;
    }
}