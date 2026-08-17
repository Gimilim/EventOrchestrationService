namespace EventOrchestrationService.Domain.Exceptions;

public class ConcurrencyException(string message) : DomainException(message);