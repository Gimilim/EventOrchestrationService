using EventOrchestrationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<User> Users => Set<User>();

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

        // просто для удобства данные для проверки через сваггер. Выключены для тестов
        if (Database.IsNpgsql())
        {
            modelBuilder.Entity<Event>().HasData(SeedData.SeedData.GetEvents());
        }
    }
}