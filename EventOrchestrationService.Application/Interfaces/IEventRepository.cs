using EventOrchestrationService.Entities;

namespace EventOrchestrationService.Application.Interfaces;

public interface IEventRepository
{
    IQueryable<Event> Query();
    Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Event newEvent, CancellationToken cancellationToken = default);
    Task DeleteAsync(Event targetEvent);
    string? GetDatabaseProviderName();
    IQueryable<Event> FilterEvents(string? title, DateTime? from, DateTime? to);
}