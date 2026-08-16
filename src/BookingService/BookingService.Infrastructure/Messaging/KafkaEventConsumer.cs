using Confluent.Kafka;
using EventOrchestrationService.Contracts.Events;
using BookingService.Application.Interfaces;
using BookingService.Application.Settings;
using BookingService.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace BookingService.Infrastructure.Messaging;

public class KafkaEventConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaEventConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AsyncRetryPolicy _retryPolicy;

    public KafkaEventConsumer(
        IOptions<KafkaSettings> options,
        ILogger<KafkaEventConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        var settings = options.Value;

        var config = new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = "booking-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = true
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe("booking-confirmed");
        _consumer.Subscribe("booking-rejected");
        _consumer.Subscribe("booking-cancelled");

        _retryPolicy = Policy
            .Handle<Exception>(IsTransientException)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogRetry(retryCount, (int)timeSpan.TotalSeconds);
                });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                if (consumeResult?.Message?.Value == null) continue;

                using var scope = _scopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                switch (consumeResult.Topic)
                {
                    case "booking-confirmed":
                    {
                        var evt = JsonSerializer.Deserialize<BookingConfirmedEvent>(consumeResult.Message.Value);
                        if (evt == null) continue;

                        _logger.LogBookingConfirmedReceived(evt.BookingId);

                        await _retryPolicy.ExecuteAsync(async () =>
                        {
                            var booking = await bookingRepository.GetByIdAsync(evt.BookingId, stoppingToken);
                            if (booking == null)
                            {
                                _logger.LogWarning("Бронь {BookingId} не найдена", evt.BookingId);
                                return;
                            }

                            booking.Confirm();
                            await bookingRepository.SaveChangesAsync(stoppingToken);
                        });

                        _consumer.Commit(consumeResult);
                        _logger.LogBookingConfirmedProcessed(evt.BookingId);
                        break;
                    }
                    case "booking-rejected":
                    {
                        var evt = JsonSerializer.Deserialize<BookingRejectedEvent>(consumeResult.Message.Value);
                        if (evt == null) continue;

                        _logger.LogBookingRejectedReceived(evt.BookingId, evt.Reason);

                        await _retryPolicy.ExecuteAsync(async () =>
                        {
                            var booking = await bookingRepository.GetByIdAsync(evt.BookingId, stoppingToken);
                            if (booking == null)
                            {
                                _logger.LogBookingNotFound(evt.BookingId);
                                return;
                            }

                            booking.Reject();
                            await bookingRepository.SaveChangesAsync(stoppingToken);
                        });

                        _consumer.Commit(consumeResult);
                        _logger.LogBookingRejectedProcessed(evt.BookingId, evt.Reason);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке события");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private static bool IsTransientException(Exception ex)
    {
        return ex is TimeoutException ||
               ex is OperationCanceledException ||
               (ex is KafkaException kafkaEx && IsTransientKafkaError(kafkaEx.Error.Code));
    }

    private static bool IsTransientKafkaError(ErrorCode errorCode)
    {
        return errorCode == ErrorCode.Local_Transport ||
               errorCode == ErrorCode.Local_AllBrokersDown ||
               errorCode == ErrorCode.Local_QueueFull ||
               errorCode == ErrorCode.Local_MsgTimedOut ||
               errorCode == ErrorCode.BrokerNotAvailable ||
               errorCode == ErrorCode.RequestTimedOut;
    }

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}