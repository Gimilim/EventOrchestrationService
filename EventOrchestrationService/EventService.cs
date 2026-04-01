using EventOrchestrationService.Models;

namespace EventOrchestrationService;

public class EventService : IEventService
{
    // Имитация базы данных
    private static List<Event> _events = new()
    {
        new Event
        {
            Id = 1, Title = "Title1", Description = "Description1", StartAt = DateTime.Now.AddDays(-5),
            EndAt = DateTime.Now
        },
        new Event
        {
            Id = 2, Title = "Title1", Description = "Description1", StartAt = DateTime.Now.AddDays(-5),
            EndAt = DateTime.Now
        },
    };

    public List<Event> GetAllEvents()
    {
        return _events;
    }

    public Event? GetEventById(int id)
    {
        return _events.FirstOrDefault(o => o.Id == id);
    }

    public Event CreateEvent(Event newEvent)
    {
        newEvent.Id = _events.Any() ? _events.Max(o => o.Id) + 1 : 1;
        _events.Add(newEvent);

        return newEvent;
    }

    public Event? UpdateEvent(int id, Event updatedEvent)
    {
        var existingEvent = _events.FirstOrDefault(o => o.Id == id);

        if (existingEvent == null)
        {
            return null;
        }

        existingEvent.Title = updatedEvent.Title;
        existingEvent.Description = updatedEvent.Description;
        existingEvent.StartAt = updatedEvent.StartAt;
        existingEvent.EndAt = updatedEvent.EndAt;

        return existingEvent;
    }

    public bool DeleteEvent(int id)
    {
        var targetEvent = _events.FirstOrDefault(o => o.Id == id);
        if (targetEvent == null)
        {
            return false;
        }

        _events.Remove(targetEvent);
        return true;
    }
}