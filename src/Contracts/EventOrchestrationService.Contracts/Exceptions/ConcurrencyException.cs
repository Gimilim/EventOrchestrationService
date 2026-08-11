namespace EventOrchestrationService.Contracts.Exceptions;

public class ConcurrencyException(string message) : DomainException(message);