using EventOrchestrationService.Contracts.Exceptions;

namespace EventService.Domain.Exceptions;

public class EventAlreadyStartedException(string message) : DomainException(message);