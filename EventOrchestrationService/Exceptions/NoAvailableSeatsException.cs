namespace EventOrchestrationService.Exceptions;

public class NoAvailableSeatsException(string message) : Exception(message);