using EventOrchestrationService.Models;
using EventOrchestrationService.Models.DTO;

namespace EventOrchestrationService;

public interface IEventService
{
    PaginatedResult GetEvents(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10);
    Event? GetEventById(int id);
    Task<Event?> GetEventByIdAsync(int id, CancellationToken cancellationToken);
    Task<Event> CreateEventAsync(Event newEvent, CancellationToken cancellationToken);
    Event? UpdateEvent(int id, Event updatedEvent);
    Task<Event?> UpdateEventAsync(int id, Event updatedEvent, CancellationToken cancellationToken);
    bool DeleteEvent(int id);
}