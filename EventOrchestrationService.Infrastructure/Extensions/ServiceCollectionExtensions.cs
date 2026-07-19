using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Infrastructure.BackgroundServices;
using EventOrchestrationService.Infrastructure.Data;
using EventOrchestrationService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventOrchestrationService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("PostgresqlConnection")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'PostgresqlConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            if (environment.IsDevelopment())
            {
                options.LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging();
            }
        });

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddHostedService<BookingBackgroundService>();

        return services;
    }
}