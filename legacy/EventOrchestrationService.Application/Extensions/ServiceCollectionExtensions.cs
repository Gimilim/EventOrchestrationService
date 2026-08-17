using EventOrchestrationService.Application.Services;
using EventOrchestrationService.Application.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EventOrchestrationService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}