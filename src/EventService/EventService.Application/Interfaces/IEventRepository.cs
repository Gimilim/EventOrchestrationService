using EventOrchestrationService.Contracts.DTOs;
using EventService.Domain.Entities;

namespace EventService.Application.Interfaces;

public interface IEventRepository
{
    IQueryable<Event> Query();
    Task<List<Event>> GetTop10Async(CancellationToken cancellationToken = default);
    Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Event newEvent, CancellationToken cancellationToken = default);
    Task DeleteAsync(Event targetEvent);
    string? GetDatabaseProviderName();
    IQueryable<Event> FilterEvents(string? title, DateTime? from, DateTime? to);

    Task<(List<Event> Items, int TotalCount)> GetPagedEventsAsync(string? title = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}