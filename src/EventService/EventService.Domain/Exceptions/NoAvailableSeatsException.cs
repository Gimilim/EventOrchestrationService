using EventOrchestrationService.Contracts.Exceptions;

namespace EventService.Domain.Exceptions;

public class NoAvailableSeatsException(string message) : DomainException(message);