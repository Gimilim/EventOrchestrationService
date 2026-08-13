using EventOrchestrationService.Contracts.Enums;
using EventOrchestrationService.Contracts.Exceptions;
using FluentValidation;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using ValidationException = EventOrchestrationService.Contracts.Exceptions.ValidationException;

namespace UserService.Application.Services;

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

        //todo покрыть тестами данный сценарий
        var existingUser = await userRepository.GetByLoginAsync(registerData.Login, cancellationToken);
        if (existingUser is not null)
            throw new ValidationException("Пользователь с таким логином уже существует.");

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