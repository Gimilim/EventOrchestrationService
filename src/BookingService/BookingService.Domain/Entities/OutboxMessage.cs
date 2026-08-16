namespace BookingService.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Topic { get; private set; }
    public string? Key { get; private set; }
    public string Payload { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int Attempts { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(string topic, string payload, string? key = null)
    {
        Id = Guid.NewGuid();
        Topic = topic;
        Key = key;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
        Attempts = 0;
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
    }

    public void IncrementAttempts()
    {
        Attempts++;
    }
}