namespace EventOrchestrationService.Contracts.DTOs;

public class UpdateEventContractDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int? TotalSeats { get; set; }
}