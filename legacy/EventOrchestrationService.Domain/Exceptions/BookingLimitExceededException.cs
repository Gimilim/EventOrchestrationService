namespace EventOrchestrationService.Domain.Exceptions;

public class BookingLimitExceededException(string message) : DomainException(message);