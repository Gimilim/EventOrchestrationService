using EventOrchestrationService.Application.DTOs;
using EventOrchestrationService.Entities;

namespace EventOrchestrationService.Application.Interfaces;

public interface IEventService
{
    Task<PaginatedResult> GetEventsAsync(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<Event?> GetEventByIdAsync(int id, CancellationToken cancellationToken);
    Task<Event> CreateEventAsync(Event newEvent, CancellationToken cancellationToken);
    Task<Event?> UpdateEventAsync(int id, Event updatedEvent, CancellationToken cancellationToken);
    Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken);
}