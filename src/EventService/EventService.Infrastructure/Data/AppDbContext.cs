using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // хак для тестов
        if (Database.IsSqlite())
        {
            var propertyBuilder = modelBuilder.Entity<Event>().Property(e => e.RowVersion);
            propertyBuilder.Metadata.IsConcurrencyToken = false;
            propertyBuilder.ValueGeneratedNever(); 
        }
    }
}