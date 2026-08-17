using BookingService.Application.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, Services.BookingService>();

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}