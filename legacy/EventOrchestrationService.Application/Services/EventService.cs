using EventOrchestrationService.Application.DTOs;
using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Domain.Entities;
using FluentValidation;

namespace EventOrchestrationService.Application.Services;

public class EventService(
    IValidator<CreateEventDto> createValidator,
    IValidator<UpdateEventDto> updateValidator,
    IEventRepository eventRepository)
    : IEventService
{
    public async Task<PaginatedResult<Event>> GetEventsAsync(string? title = null, DateTime? from = null,
        DateTime? to = null,
        int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await eventRepository.GetPagedEventsAsync(
            title, from, to, page, pageSize, cancellationToken);

        return new PaginatedResult<Event>
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

    public async Task<Event> CreateEventAsync(CreateEventDto dto, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var newEvent = new Event(dto.Title, dto.Description, dto.StartAt, dto.EndAt, dto.TotalSeats);

        await eventRepository.AddAsync(newEvent, cancellationToken);
        await eventRepository.SaveChangesAsync(cancellationToken);

        return newEvent;
    }

    public async Task<Event?> UpdateEventAsync(int id, UpdateEventDto dto, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var existingEvent = await eventRepository.GetByIdAsync(id, cancellationToken);

        if (existingEvent == null)
        {
            return null;
        }

        existingEvent.Update(
            dto.Title,
            dto.Description,
            dto.StartAt,
            dto.EndAt,
            dto.TotalSeats
        );

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