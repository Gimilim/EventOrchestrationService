namespace EventOrchestrationService.Domain.Exceptions;

public class EventAlreadyStartedException(string message) : DomainException(message);