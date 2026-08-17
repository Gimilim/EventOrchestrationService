namespace EventOrchestrationService.Contracts.Exceptions;

public class NotFoundException(string message) : DomainException(message);