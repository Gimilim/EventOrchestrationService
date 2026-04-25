using EventOrchestrationService.Data;
using EventOrchestrationService.Models;
using EventOrchestrationService.Models.DTO;

namespace EventOrchestrationService;

public class EventService(AppDbContext context) : IEventService
{
    public PaginatedResult GetEvents(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10)
    {
        IQueryable<Event> query = context.Events;

        if (!string.IsNullOrEmpty(title))
        {
            // todo временная мера. Не оптимально, но сейчас лучше не сделать
            query = query
                .Where(e => e.Title.ToLower().Contains(title.ToLower()));
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

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalCount = query.Count();

        return new PaginatedResult
        {
            TotalCount = totalCount,
            Items = items,
            Page = page,
            PageSize = items.Count
        };
    }

    public Event? GetEventById(int id)
    {
        return context.Events.FirstOrDefault(o => o.Id == id);
    }

    public Event CreateEvent(Event newEvent)
    {
        context.Events.Add(newEvent);

        context.SaveChanges();
        return newEvent;
    }

    public Event? UpdateEvent(int id, Event updatedEvent)
    {
        var existingEvent = context.Events.FirstOrDefault(o => o.Id == id);

        if (existingEvent == null)
        {
            return null;
        }

        existingEvent.Title = updatedEvent.Title;
        existingEvent.Description = updatedEvent.Description;
        existingEvent.StartAt = updatedEvent.StartAt;
        existingEvent.EndAt = updatedEvent.EndAt;

        context.SaveChanges();
        return existingEvent;
    }

    public bool DeleteEvent(int id)
    {
        var targetEvent = context.Events.FirstOrDefault(o => o.Id == id);
        if (targetEvent == null)
        {
            return false;
        }

        context.Events.Remove(targetEvent);
        context.SaveChanges();
        return true;
    }
}