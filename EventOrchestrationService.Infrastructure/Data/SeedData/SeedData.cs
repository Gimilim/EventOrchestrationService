using EventOrchestrationService.Domain.Entities;

namespace EventOrchestrationService.Infrastructure.Data.SeedData;

public static class SeedData
{
    public static List<Event> GetEvents()
    {
        return new List<Event>
        {
            new Event(
                title: "Title1",
                description: "Description1",
                startAt: DateTime.UtcNow.AddDays(-5),
                endAt: DateTime.UtcNow.AddDays(3),
                totalSeats: 10
            ),
            new Event(
                title: "Title2",
                description: "Description2",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                title: "Title3",
                description: "Description3",
                startAt: DateTime.UtcNow.AddDays(-8),
                endAt: DateTime.UtcNow.AddDays(5),
                totalSeats: 10
            ),
            new Event(
                title: "ABC_Title4",
                description: "Description4",
                startAt: DateTime.UtcNow.AddDays(-8),
                endAt: DateTime.UtcNow.AddDays(5),
                totalSeats: 10
            ),
            new Event(
                title: "abc_Title5",
                description: "Description5",
                startAt: new DateTime(2055, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: DateTime.UtcNow.AddDays(5),
                totalSeats: 10
            ),
            new Event(
                title: "AbC_Title6",
                description: "Description6",
                startAt: new DateTime(2055, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2077, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                title: "Title7",
                description: "Description7",
                startAt: new DateTime(2055, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2077, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                title: "Title8",
                description: "Description8",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2077, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                title: "Title9",
                description: "Description9",
                startAt: new DateTime(2027, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2027, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
        };
    }
}