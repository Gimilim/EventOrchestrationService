namespace EventOrchestrationService.Application.DTOs;

public class UpdateEventDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int? TotalSeats { get; set; }
}