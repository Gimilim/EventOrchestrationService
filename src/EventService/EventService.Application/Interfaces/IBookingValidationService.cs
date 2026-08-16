using EventOrchestrationService.Contracts.Events;

namespace EventService.Application.Interfaces;

public interface IBookingValidationService
{
    Task ValidateBookingAsync(BookingCreatedEvent evt, CancellationToken cancellationToken);
}