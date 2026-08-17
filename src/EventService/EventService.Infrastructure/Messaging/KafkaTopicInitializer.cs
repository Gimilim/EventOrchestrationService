using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventService.Application.Settings;
using EventService.Infrastructure.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventService.Infrastructure.Messaging;

public class KafkaTopicInitializer(IOptions<KafkaSettings> options, ILogger<KafkaTopicInitializer> logger)
    : IHostedService
{
    private readonly KafkaSettings _settings = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var topics = new List<string>
        {
            "booking-created",
            "booking-cancelled"
        };

        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = _settings.BootstrapServers
        }).Build();

        try
        {
            var existingTopics = adminClient.GetMetadata(TimeSpan.FromSeconds(10)).Topics
                .Select(t => t.Topic)
                .ToHashSet();

            var topicsToCreate = topics
                .Where(t => !existingTopics.Contains(t))
                .Select(t => new TopicSpecification
                {
                    Name = t,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                })
                .ToList();

            if (topicsToCreate.Count != 0)
            {
                await adminClient.CreateTopicsAsync(topicsToCreate, new CreateTopicsOptions
                {
                    OperationTimeout = TimeSpan.FromSeconds(30)
                });

                var topicNames = string.Join(", ", topicsToCreate.Select(t => t.Name));
                logger.LogTopicsCreated(topicNames);
            }
            else
            {
                logger.LogTopicsAlreadyExist();
            }
        }
        catch (Exception ex)
        {
            logger.LogTopicsCreationError(ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}