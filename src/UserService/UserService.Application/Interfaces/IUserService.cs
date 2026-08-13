using UserService.Application.DTOs;

namespace UserService.Application.Interfaces;

public interface IUserService
{
    Task RegisterAsync(RegisterDataDto registerData, CancellationToken cancellationToken);
    Task<string> LoginAsync(LoginDataDto loginData, CancellationToken cancellationToken);
}