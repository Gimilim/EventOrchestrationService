using EventOrchestrationService.Contracts.DTOs;

namespace EventOrchestrationService.Contracts.Interfaces;

public interface IEventService
{
    Task<PaginatedResult<EventContractDto>> GetEventsAsync(string? title = null, DateTime? from = null, DateTime? to = null,
        int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    Task<EventContractDto?> GetEventByIdAsync(int id, CancellationToken cancellationToken);
    Task<EventContractDto> CreateEventAsync(CreateEventContractDto newEvent, CancellationToken cancellationToken);
    Task<EventContractDto?> UpdateEventAsync(int id, UpdateEventContractDto updatedEvent, CancellationToken cancellationToken);
    Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken);
}