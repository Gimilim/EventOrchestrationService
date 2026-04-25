using EventOrchestrationService.Models;
using EventOrchestrationService.Models.DTO;

namespace EventOrchestrationService;

public interface IEventService
{
    PaginatedResult GetEvents(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10);
    Event? GetEventById(int id);
    Event CreateEvent(Event newEvent);
    Event? UpdateEvent(int id, Event updatedEvent);
    bool DeleteEvent(int id);
}