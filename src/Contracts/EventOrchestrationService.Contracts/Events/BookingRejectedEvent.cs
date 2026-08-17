namespace EventOrchestrationService.Contracts.Events;

public class BookingRejectedEvent
{
    public int BookingId { get; set; }
    public string Reason { get; set; } = string.Empty;
}