namespace EventOrchestrationService.Contracts.Events;

public class BookingCancelledEvent
{
    public int BookingId { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
}