using EventOrchestrationService.Data.Repositories.Interfaces;
using EventOrchestrationService.DTOs;
using EventOrchestrationService.Entities;
using EventOrchestrationService.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.Services.Implementations;

public class EventService(IValidator<Event> validator, IEventRepository eventRepository)
    : IEventService
{
    private void Validate(Event eventToValidate)
    {
        var result = validator.Validate(eventToValidate);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    public async Task<PaginatedResult> GetEventsAsync(string? title = null, DateTime? from = null, DateTime? to = null,
        int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = eventRepository.FilterEvents(title, from, to);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        return new PaginatedResult
        {
            TotalCount = totalCount,
            Items = items,
            Page = page,
            PageSize = items.Count
        };
    }

    public async Task<Event?> GetEventByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await eventRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Event> CreateEventAsync(Event newEvent, CancellationToken cancellationToken)
    {
        newEvent.AvailableSeats = newEvent.TotalSeats;
        Validate(newEvent);

        await eventRepository.AddAsync(newEvent, cancellationToken);
        await eventRepository.SaveChangesAsync(cancellationToken);

        return newEvent;
    }

    public async Task<Event?> UpdateEventAsync(int id, Event updatedEvent, CancellationToken cancellationToken)
    {
        Validate(updatedEvent);

        var existingEvent = await eventRepository.GetByIdAsync(id, cancellationToken);

        if (existingEvent == null)
        {
            return null;
        }

        existingEvent.Title = updatedEvent.Title;
        existingEvent.Description = updatedEvent.Description;
        existingEvent.StartAt = updatedEvent.StartAt;
        existingEvent.EndAt = updatedEvent.EndAt;
        existingEvent.TotalSeats = updatedEvent.TotalSeats;
        existingEvent.AvailableSeats = updatedEvent.AvailableSeats;

        await eventRepository.SaveChangesAsync(cancellationToken);

        return existingEvent;
    }

    public async Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken)
    {
        var targetEvent = await eventRepository.GetByIdAsync(id, cancellationToken);
        if (targetEvent == null)
        {
            return false;
        }

        await eventRepository.DeleteAsync(targetEvent);
        await eventRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}