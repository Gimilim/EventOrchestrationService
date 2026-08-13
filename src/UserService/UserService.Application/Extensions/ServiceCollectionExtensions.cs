using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces;

namespace EventOrchestrationService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService.Application.Services.UserService>();

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}