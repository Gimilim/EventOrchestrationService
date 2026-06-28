using EventOrchestrationService.Data.Repositories.Interfaces;
using EventOrchestrationService.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.Data.Repositories.Implementations;

public class EventRepository(AppDbContext dbContext) : IEventRepository
{
    public IQueryable<Event> Query()
    {
        return dbContext.Events;
    }

    public async Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.Events.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(Event newEvent, CancellationToken cancellationToken = default)
    {
        await dbContext.Events.AddAsync(newEvent, cancellationToken);
    }

    public Task DeleteAsync(Event targetEvent)
    {
        dbContext.Events.Remove(targetEvent);
        return Task.CompletedTask;
    }

    public string? GetDatabaseProviderName() => dbContext.Database.ProviderName;

    public IQueryable<Event> FilterEvents(string? title, DateTime? from, DateTime? to)
    {
        var query = dbContext.Events.AsQueryable();

        if (!string.IsNullOrEmpty(title))
        {
            query = dbContext.Database.ProviderName?.Contains("Npgsql") == true
                ? query.Where(e => EF.Functions.ILike(e.Title, $"%{title}%"))
                : query.Where(e => e.Title.ToLower().Contains(title.ToLower()));
        }

        if (from.HasValue)
        {
            query = query.Where(e => e.StartAt >= from);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.EndAt <= to);
        }

        return query;
    }
}