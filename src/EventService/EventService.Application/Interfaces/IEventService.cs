using EventOrchestrationService.Contracts.DTOs;
using EventService.Domain.Entities;

namespace EventService.Application.Interfaces;

public interface IEventService
{
    Task<PaginatedResult<EventContractDto>> GetEventsAsync(string? title = null, DateTime? from = null, DateTime? to = null,
        int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    Task<EventContractDto?> GetEventByIdAsync(int id, CancellationToken cancellationToken);
    Task<EventContractDto> CreateEventAsync(CreateEventContractDto newEvent, CancellationToken cancellationToken);
    Task<EventContractDto?> UpdateEventAsync(int id, UpdateEventContractDto updatedEvent, CancellationToken cancellationToken);
    Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken);
    Task<List<EventContractDto>> GetTop10Async(CancellationToken cancellationToken);
}