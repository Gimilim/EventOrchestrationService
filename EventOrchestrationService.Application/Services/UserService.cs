using EventOrchestrationService.Application.DTOs;
using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Domain.Entities;
using EventOrchestrationService.Domain.Enums;
using FluentValidation;

namespace EventOrchestrationService.Application.Services;

public class UserService(
    IValidator<RegisterDataDto> registerDataValidator,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : IUserService
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
}