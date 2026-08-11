namespace EventOrchestrationService.Contracts.Exceptions;

public class BadRequestException(string message) : DomainException(message);