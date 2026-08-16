using BookingService.Application.Interfaces;
using BookingService.Application.Settings;
using BookingService.Infrastructure.BackgroundServices;
using BookingService.Infrastructure.Data;
using BookingService.Infrastructure.Data.Repositories;
using BookingService.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.Extensions;

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

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IInboxRepository, InboxRepository>();

        services.Configure<BookingSettings>(configuration.GetSection("BookingSettings"));

        // Блок Кафки
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddHostedService<KafkaEventConsumer>();
        services.AddHostedService<OutboxProcessor>();
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        services.AddHostedService<BookingBackgroundService>();

        return services;
    }
}