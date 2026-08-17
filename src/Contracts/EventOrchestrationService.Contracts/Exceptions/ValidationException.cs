namespace EventOrchestrationService.Contracts.Exceptions;

public class ValidationException(string message) : DomainException(message);