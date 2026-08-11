namespace EventOrchestrationService.Contracts.Exceptions;

public class UnauthorizedException(string message) : DomainException(message);