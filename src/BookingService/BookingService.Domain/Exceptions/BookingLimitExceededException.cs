using EventOrchestrationService.Contracts.Exceptions;

namespace BookingService.Domain.Exceptions;

public class BookingLimitExceededException(string message) : DomainException(message);