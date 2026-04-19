using EventOrchestrationService.Models;
using EventOrchestrationService.Models.DTO;

namespace EventOrchestrationService;

public interface IEventService
{
    PaginatedResult GetEvents(string? title, DateTime? from, DateTime? to, int page, int pageSize);
    Event? GetEventById(int id);
    Event CreateEvent(Event newEvent);
    Event? UpdateEvent(int id, Event updatedEvent);
    bool DeleteEvent(int id);
}