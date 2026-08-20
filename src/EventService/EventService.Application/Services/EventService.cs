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
    IMapper mapper,
    ICacheService cache,
    IUnitOfWork unitOfWork)
    : IEventService
{
    private const string CacheKeyPrefix = "event";

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
        var cacheKey = $"{CacheKeyPrefix}:{id}";

        var cached = await cache.GetAsync<EventContractDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var existingEvent = await eventRepository.GetByIdAsync(id, cancellationToken);

        if (existingEvent == null)
        {
            return null;
        }

        var result = mapper.Map<EventContractDto>(existingEvent);

        await cache.SetAsync(cacheKey, result, cancellationToken: cancellationToken);

        return result;
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
        EventContractDto? result = null;

        await using (var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken))
        {
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

            result = mapper.Map<EventContractDto>(existingEvent);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        await cache.RemoveAsync($"{CacheKeyPrefix}:{id}", cancellationToken);
        return result;
    }

    public async Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken)
    {
        await using (var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken))
        {
            var targetEvent = await eventRepository.GetByIdAsync(id, cancellationToken);
            if (targetEvent == null)
            {
                return false;
            }

            await eventRepository.DeleteAsync(targetEvent);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        await cache.RemoveAsync($"{CacheKeyPrefix}:{id}", cancellationToken);
        return true;
    }
}