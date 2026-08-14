using System.Text.Json;
using Confluent.Kafka;
using BookingService.Application.Interfaces;
using BookingService.Application.Settings;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Messaging;

public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> _producer;

    public KafkaEventPublisher(IOptions<KafkaSettings> options)
    {
        var settings = options.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
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

        await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}