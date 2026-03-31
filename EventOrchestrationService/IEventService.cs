using EventOrchestrationService.Models;

namespace EventOrchestrationService;

public interface IEventService
{
    List<Event> GetAllEvents();
    Event? GetEventById(int id);
    Event CreateEvent(Event newEvent);
    Event? UpdateEvent(int id, Event updatedEvent);
    bool DeleteEvent(int id);
}