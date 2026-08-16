using EventService.Application.Interfaces;
using EventService.Application.Services;
using EventService.Application.Settings;
using EventService.Infrastructure.Data;
using EventService.Infrastructure.Data.Repositories;
using EventService.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventService.Infrastructure.Extensions;

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

        // Блок Кафки
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddHostedService<KafkaEventConsumer>();
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        services.AddScoped<IBookingValidationService, BookingValidationService>();

        return services;
    }
}