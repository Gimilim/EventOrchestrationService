using EventOrchestrationService.Models;
using EventOrchestrationService.Models.DTO;

namespace EventOrchestrationService;

public class EventService : IEventService
{
    // Имитация базы данных
    private static List<Event> _events = new()
    {
        new Event
        {
            Id = 1, Title = "Title1", Description = "Description1", StartAt = DateTime.Now.AddDays(-5),
            EndAt = DateTime.Now.AddDays(3)
        },
        new Event
        {
            Id = 2, Title = "Title2", Description = "Description2", StartAt = new DateTime(2025, 1, 30),
            EndAt = new DateTime(2025, 12, 30)
        },
        new Event
        {
            Id = 3, Title = "Title3", Description = "Description3", StartAt = DateTime.Now.AddDays(-8),
            EndAt = DateTime.Now.AddDays(5)
        },
        new Event
        {
            Id = 4, Title = "ABC_Title4", Description = "Description4", StartAt = DateTime.Now.AddDays(-8),
            EndAt = DateTime.Now.AddDays(5)
        },
        new Event
        {
            Id = 5, Title = "abc_Title5", Description = "Description5", StartAt = new DateTime(2055, 1, 30),
            EndAt = DateTime.Now.AddDays(5)
        },
        new Event
        {
            Id = 6, Title = "AbC_Title6", Description = "Description6", StartAt = new DateTime(2055, 1, 30),
            EndAt = new DateTime(2077, 12, 30)
        },
        new Event
        {
            Id = 7, Title = "Title7", Description = "Description7", StartAt = new DateTime(2055, 1, 30),
            EndAt = new DateTime(2077, 12, 30)
        },
        new Event
        {
            Id = 8, Title = "Title8", Description = "Description8", StartAt = new DateTime(2025, 1, 30),
            EndAt = new DateTime(2077, 12, 30)
        },
        new Event
        {
            Id = 9, Title = "Title9", Description = "Description9", StartAt = new DateTime(2027, 1, 30),
            EndAt = new DateTime(2027, 12, 30)
        },
    };

    public PaginatedResult GetEvents(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10)
    {
        var query = _events.AsEnumerable();

        if (!string.IsNullOrEmpty(title))
        {
            query = query
                .Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (from.HasValue)
        {
            query = query
                .Where(e => e.StartAt >= from);
        }

        if (to.HasValue)
        {
            query = query
                .Where(e => e.EndAt <= to);
        }

        var filtered = query.ToList();

        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult
        {
            TotalCount = filtered.Count,
            Items = items,
            Page = page,
            PageSize = items.Count
        };
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