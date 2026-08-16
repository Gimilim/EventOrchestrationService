namespace BookingService.Domain.Entities;

public class InboxMessage
{
    public Guid Id { get; private set; }
    public string EventId { get; private set; }
    public string Topic { get; private set; }
    public string Payload { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    private InboxMessage() { }

    public InboxMessage(string eventId, string topic, string payload)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        Topic = topic;
        Payload = payload;
        ProcessedAt = DateTime.UtcNow;
    }
}