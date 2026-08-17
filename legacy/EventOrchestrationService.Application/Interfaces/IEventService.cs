using EventOrchestrationService.Application.DTOs;
using EventOrchestrationService.Domain.Entities;

namespace EventOrchestrationService.Application.Interfaces;

public interface IEventService
{
    Task<PaginatedResult<Event>> GetEventsAsync(string? title = null, DateTime? from = null, DateTime? to = null,
        int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    Task<Event?> GetEventByIdAsync(int id, CancellationToken cancellationToken);
    Task<Event> CreateEventAsync(CreateEventDto newEvent, CancellationToken cancellationToken);
    Task<Event?> UpdateEventAsync(int id, UpdateEventDto updatedEvent, CancellationToken cancellationToken);
    Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken);
}