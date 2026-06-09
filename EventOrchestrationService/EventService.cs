using EventOrchestrationService.Data;
using EventOrchestrationService.Exceptions;
using EventOrchestrationService.Models;
using EventOrchestrationService.Models.DTO;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService;

public class EventService(AppDbContext dbContext, IValidator<Event> validator) : IEventService
{
    private void Validate(Event eventToValidate)
    {
        var result = validator.Validate(eventToValidate);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    public PaginatedResult GetEvents(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10)
    {
        IQueryable<Event> query = dbContext.Events;

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

    // todo уберу на следующем рефакторе, оставлю только async метод 
    public Event? GetEventById(int id)
    {
        return dbContext.Events.FirstOrDefault(o => o.Id == id);
    }

    public async Task<Event?> GetEventByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await dbContext.Events.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (result == null)
            throw new NotFoundException($"Событие с ID {id} не найдено");

        return result;
    }

    public async Task<Event> CreateEventAsync(Event newEvent, CancellationToken cancellationToken)
    {
        newEvent.AvailableSeats = newEvent.TotalSeats;
        Validate(newEvent);

        await dbContext.Events.AddAsync(newEvent, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return newEvent;
    }

    public Event? UpdateEvent(int id, Event updatedEvent)
    {
        Validate(updatedEvent);

        var existingEvent = dbContext.Events.FirstOrDefault(o => o.Id == id);

        if (existingEvent == null)
        {
            return null;
        }

        existingEvent.Title = updatedEvent.Title;
        existingEvent.Description = updatedEvent.Description;
        existingEvent.StartAt = updatedEvent.StartAt;
        existingEvent.EndAt = updatedEvent.EndAt;

        dbContext.SaveChanges();
        return existingEvent;
    }

    public bool DeleteEvent(int id)
    {
        var targetEvent = dbContext.Events.FirstOrDefault(o => o.Id == id);
        if (targetEvent == null)
        {
            return false;
        }

        dbContext.Events.Remove(targetEvent);
        dbContext.SaveChanges();
        return true;
    }
}