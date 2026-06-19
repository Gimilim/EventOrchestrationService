using EventOrchestrationService.Models;

namespace EventOrchestrationService.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        if (context.Events.Any())
        {
            return;
        }

        var events = new List<Event>
        {
            new()
            {
                Id = 1, Title = "Title1", Description = "Description1", StartAt = DateTime.UtcNow.AddDays(-5),
                EndAt = DateTime.UtcNow.AddDays(3), TotalSeats = 10, AvailableSeats = 10
            },
            new()
            {
                Id = 2, Title = "Title2", Description = "Description2", StartAt = new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc), TotalSeats = 10, AvailableSeats = 10
            },
            new()
            {
                Id = 3, Title = "Title3", Description = "Description3", StartAt = DateTime.UtcNow.AddDays(-8),
                EndAt = DateTime.UtcNow.AddDays(5), TotalSeats = 10, AvailableSeats = 10
            },
            new()
            {
                Id = 4, Title = "ABC_Title4", Description = "Description4", StartAt = DateTime.UtcNow.AddDays(-8),
                EndAt = DateTime.UtcNow.AddDays(5), TotalSeats = 10, AvailableSeats = 10
            },
            new()
            {
                Id = 5, Title = "abc_Title5", Description = "Description5", StartAt = new DateTime(2055, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                EndAt = DateTime.UtcNow.AddDays(5), TotalSeats = 10, AvailableSeats = 10
            },
            new()
            {
                Id = 6, Title = "AbC_Title6", Description = "Description6", StartAt = new DateTime(2055, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2077, 12, 30, 0, 0, 0, DateTimeKind.Utc), TotalSeats = 10, AvailableSeats = 10
            },
            new()
            {
                Id = 7, Title = "Title7", Description = "Description7", StartAt = new DateTime(2055, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2077, 12, 30, 0, 0, 0, DateTimeKind.Utc), TotalSeats = 10, AvailableSeats = 10
            },
            new()
            {
                Id = 8, Title = "Title8", Description = "Description8", StartAt = new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2077, 12, 30, 0, 0, 0, DateTimeKind.Utc), TotalSeats = 10, AvailableSeats = 10
            },
            new()
            {
                Id = 9, Title = "Title9", Description = "Description9", StartAt = new DateTime(2027, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2027, 12, 30, 0, 0, 0, DateTimeKind.Utc), TotalSeats = 10, AvailableSeats = 10
            },
        };

        context.Events.AddRange(events);
        context.SaveChanges();
    }
}