using System.Text.Json;
using Confluent.Kafka;
using BookingService.Application.Interfaces;
using BookingService.Application.Settings;
using BookingService.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace BookingService.Infrastructure.Messaging;

public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IOptions<KafkaSettings> options, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        var settings = options.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 500
        };

        _producer = new ProducerBuilder<string, string>(config).Build();

        _retryPolicy = Policy
            .Handle<KafkaException>(ex => IsTransientKafkaError(ex.Error.Code))
            .Or<OperationCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogPublishRetry(retryCount, (int)timeSpan.TotalSeconds, exception);
                }
            );
    }

    public async Task PublishAsync<T>(string topic, T message, string? key = null,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);
        var messageKey = key ?? Guid.NewGuid().ToString();

        var kafkaMessage = new Message<string, string>
        {
            Key = messageKey,
            Value = json
        };

        await _retryPolicy.ExecuteAsync(async (ct) =>
        {
            var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage, ct);

            if (deliveryResult.Status != PersistenceStatus.Persisted)
            {
                throw new Exception($"Kafka delivery failed. Status: {deliveryResult.Status}");
            }
        }, cancellationToken);
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

    public void Dispose()
    {
        _producer?.Dispose();
    }
}