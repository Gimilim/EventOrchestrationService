namespace EventOrchestrationService.Contracts.Events;

public class BookingCreatedEvent
{
    public int BookingId { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}