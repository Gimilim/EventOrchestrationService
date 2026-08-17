using EventOrchestrationService.Domain.Entities;

namespace EventOrchestrationService.Infrastructure.Data.SeedData;

public static class SeedData
{
    public static List<Event> GetEvents()
    {
        return new List<Event>
        {
            new Event(
                id: 1,
                title: "Title1",
                description: "Description1",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                id: 2,
                title: "Title2",
                description: "Description2",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                id: 3,
                title: "Title3",
                description: "Description3",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                id: 4,
                title: "ABC_Title4",
                description: "Description4",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                id: 5,
                title: "abc_Title5",
                description: "Description5",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                id: 6,
                title: "AbC_Title6",
                description: "Description6",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                id: 7,
                title: "Title7",
                description: "Description7",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                id: 8,
                title: "Title8",
                description: "Description8",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                id: 9,
                title: "Title9",
                description: "Description9",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
        };
    }
}