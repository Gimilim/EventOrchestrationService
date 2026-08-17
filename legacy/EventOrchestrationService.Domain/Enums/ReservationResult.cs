namespace EventOrchestrationService.Domain.Enums;

public enum ReservationResult
{
    Success = 1,
    NoAvailableSeats = 2,
    EventAlreadyStarted = 3
}