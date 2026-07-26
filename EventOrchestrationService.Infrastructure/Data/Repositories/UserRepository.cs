using EventOrchestrationService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.Infrastructure.Data.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}