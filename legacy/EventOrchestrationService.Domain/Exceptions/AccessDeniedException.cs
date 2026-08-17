namespace EventOrchestrationService.Domain.Exceptions;

public class AccessDeniedException(string message) : DomainException(message);