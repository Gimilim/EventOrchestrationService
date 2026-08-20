using EventService.Application.Interfaces;
using EventService.Application.Mappings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, Services.EventService>();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<EventMappingProfile>();
        });

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}