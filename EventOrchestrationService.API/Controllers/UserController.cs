using EventOrchestrationService.Application.DTOs;
using EventOrchestrationService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventOrchestrationService.API.Controllers;

[ApiController]
[Route("auth")]
public class UserController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    /// <param name="registerData">Данные о новом пользователе.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Информацию о созданном бронировании.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDataDto registerData, CancellationToken cancellationToken)
    {
        await userService.RegisterAsync(registerData, cancellationToken);
        //todo Запрет на создания пользователя с админ ролью для НЕ админа

        return NoContent();
    }

    /// <summary>
    /// Вход в систему.
    /// </summary>
    /// <param name="loginData">Данные для входа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Информацию о созданном бронировании.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDataDto loginData, CancellationToken cancellationToken)
    {
        var token = await userService.LoginAsync(loginData, cancellationToken);

        return Ok(token);
    }
}