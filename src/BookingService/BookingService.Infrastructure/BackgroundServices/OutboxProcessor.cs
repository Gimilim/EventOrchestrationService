using System.Text.Json;
using BookingService.Application.Interfaces;
using BookingService.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.BackgroundServices;

public class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
    private readonly int _batchSize = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                var messages = await outboxRepository.GetUnprocessedAsync(_batchSize, stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        await eventPublisher.PublishAsync(
                            topic: message.Topic,
                            message: JsonSerializer.Deserialize<object>(message.Payload)!,
                            key: message.Key,
                            cancellationToken: stoppingToken
                        );

                        await outboxRepository.MarkAsProcessedAsync(message.Id, stoppingToken);
                        logger.LogOutboxMessageSent(message.Id, message.Topic);
                    }
                    catch (Exception ex)
                    {
                        await outboxRepository.IncrementAttemptsAsync(message.Id, stoppingToken);
                        logger.LogOutboxMessageError(ex, message.Id);
                    }
                }

                await Task.Delay(_interval, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogOutboxProcessorError(ex);
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}