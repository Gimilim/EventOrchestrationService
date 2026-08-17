namespace EventOrchestrationService.Domain.Exceptions;

public class BadRequestException(string message) : DomainException(message);