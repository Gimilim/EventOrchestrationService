namespace EventOrchestrationService.Contracts.Exceptions;

public class AccessDeniedException(string message) : DomainException(message);