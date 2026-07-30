using EventOrchestrationService.Application.DTOs;
using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Domain.Entities;
using EventOrchestrationService.Domain.Enums;
using EventOrchestrationService.Domain.Exceptions;
using FluentValidation;

namespace EventOrchestrationService.Application.Services;

public class UserService(
    IValidator<RegisterDataDto> registerDataValidator,
    IValidator<LoginDataDto> loginDataValidator,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : IUserService
{
    public async Task RegisterAsync(RegisterDataDto registerData, CancellationToken cancellationToken)
    {
        await registerDataValidator.ValidateAndThrowAsync(registerData, cancellationToken);

        var role = registerData.Role ?? Role.User;
        var hashedPassword = passwordHasher.HashPassword(registerData.Password);

        var newUser = new User(registerData.Login, hashedPassword, role);

        await userRepository.AddAsync(newUser, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> LoginAsync(LoginDataDto loginData, CancellationToken cancellationToken)
    {
        await loginDataValidator.ValidateAndThrowAsync(loginData, cancellationToken);

        var user = await userRepository.GetByLoginAsync(loginData.Login, cancellationToken);

        if (user == null)
            throw new UnauthorizedException("Неверный логин или пароль.");

        var isValid = passwordHasher.VerifyHashedPassword(user.PasswordHash, loginData.Password);

        if (!isValid)
            throw new UnauthorizedException("Неверный логин или пароль.");

        var token = jwtService.GenerateToken(user.Id, user.Login, user.Role);

        return token;
    }
}