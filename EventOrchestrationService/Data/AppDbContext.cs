using EventOrchestrationService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; }
}