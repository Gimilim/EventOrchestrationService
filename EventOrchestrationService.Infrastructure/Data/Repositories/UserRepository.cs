using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.Infrastructure.Data.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.AddAsync(user, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}