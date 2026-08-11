namespace EventOrchestrationService.Contracts.Exceptions;

public abstract class DomainException(string message) : Exception(message);