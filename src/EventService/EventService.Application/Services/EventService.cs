using AutoMapper;
using EventOrchestrationService.Contracts.DTOs;
using EventOrchestrationService.Contracts.Interfaces;
using EventService.Application.Interfaces;
using EventService.Domain.Entities;
using FluentValidation;

namespace EventService.Application.Services;

public class EventService(
    IValidator<CreateEventContractDto> createValidator,
    IValidator<UpdateEventContractDto> updateValidator,
    IEventRepository eventRepository,
    IMapper mapper)
    : IEventService
{
    public async Task<PaginatedResult<EventContractDto>> GetEventsAsync(string? title = null, DateTime? from = null,
        DateTime? to = null,
        int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await eventRepository.GetPagedEventsAsync(
            title, from, to, page, pageSize, cancellationToken);

        return new PaginatedResult<EventContractDto>
        {
            TotalCount = totalCount,
            Items = mapper.Map<List<EventContractDto>>(items),
            Page = page,
            PageSize = items.Count
        };
    }

    public async Task<EventContractDto?> GetEventByIdAsync(int id, CancellationToken cancellationToken)
    {
        var existingEvent = await eventRepository.GetByIdAsync(id, cancellationToken);
        return existingEvent == null
            ? null
            : mapper.Map<EventContractDto>(existingEvent);
    }

    public async Task<EventContractDto> CreateEventAsync(CreateEventContractDto dto, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var newEvent = mapper.Map<Event>(dto);

        await eventRepository.AddAsync(newEvent, cancellationToken);
        await eventRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<EventContractDto>(newEvent);
    }

    public async Task<EventContractDto?> UpdateEventAsync(int id, UpdateEventContractDto dto, CancellationToken cancellationToken)
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

        return mapper.Map<EventContractDto>(existingEvent);
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