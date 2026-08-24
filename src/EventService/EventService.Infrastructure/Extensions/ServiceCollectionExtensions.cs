using EventService.Application.Interfaces;
using EventService.Application.Services;
using EventService.Application.Settings;
using EventService.Infrastructure.Caching;
using EventService.Infrastructure.Data;
using EventService.Infrastructure.Data.Repositories;
using EventService.Infrastructure.Data.UnitOfWork;
using EventService.Infrastructure.Messaging;
using EventService.Infrastructure.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

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
        services.AddScoped<IInboxRepository, InboxRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Блок Кафки
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddHostedService<KafkaEventConsumer>();
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
        services.AddHostedService<KafkaTopicInitializer>();

        // Кэширование Redis
        services.Configure<RedisSettings>(configuration.GetSection("Redis"));
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var settings = configuration.GetSection("Redis").Get<RedisSettings>()
                           ?? throw new InvalidOperationException("Redis settings not found");

            var options = settings.ToConfigurationOptions();

            return ConnectionMultiplexer.Connect(options);
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddScoped<IBookingValidationService, BookingValidationService>();

        return services;
    }
}