using EventOrchestrationService.Data.Repositories.Implementations;
using EventOrchestrationService.Entities;
using EventOrchestrationService.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.IntegrationTests.Repositories;

public class EventRepositoryTests(PostgreSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task AddAsync_ValidEvent_AddsEventToDatabase()
    {
        // Arrange
        await using var context = CreateContext();

        var repository = new EventRepository(context);
        var addedEvent = new Event
        {
            Title = "Title1",
            Description = "Description1",
            StartAt = DateTime.UtcNow.AddDays(-5),
            EndAt = DateTime.UtcNow.AddDays(3),
            TotalSeats = 10,
            AvailableSeats = 10
        };

        // Act
        await repository.AddAsync(addedEvent);
        await repository.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateContext();
        var saved = await verifyContext.Events.FirstOrDefaultAsync(e => e.Id == addedEvent.Id);

        Assert.NotNull(saved);
        Assert.Equal(addedEvent.Title, saved.Title);
        Assert.Equal(addedEvent.Description, saved.Description);
        Assert.Equal(addedEvent.StartAt, saved.StartAt, TimeSpan.FromMilliseconds(1));
        Assert.Equal(addedEvent.EndAt, saved.EndAt, TimeSpan.FromMilliseconds(1));
        Assert.Equal(addedEvent.TotalSeats, saved.TotalSeats);
        Assert.Equal(addedEvent.AvailableSeats, saved.AvailableSeats);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsEvent()
    {
        // Arrange
        await using var context = CreateContext();

        var repository = new EventRepository(context);
        var addedEvent = new Event
        {
            Title = "Title1",
            Description = "Description1",
            StartAt = DateTime.UtcNow.AddDays(-5),
            EndAt = DateTime.UtcNow.AddDays(3),
            TotalSeats = 10,
            AvailableSeats = 10
        };

        // Act
        await repository.AddAsync(addedEvent);
        await repository.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateContext();
        var verifyRepository = new EventRepository(verifyContext);

        var getTargetEvent = await verifyRepository.GetByIdAsync(addedEvent.Id);

        Assert.NotNull(getTargetEvent);
        Assert.Equal(addedEvent.Title, getTargetEvent.Title);
        Assert.Equal(addedEvent.Description, getTargetEvent.Description);
        Assert.Equal(addedEvent.StartAt, getTargetEvent.StartAt, TimeSpan.FromMilliseconds(1));
        Assert.Equal(addedEvent.EndAt, getTargetEvent.EndAt, TimeSpan.FromMilliseconds(1));
        Assert.Equal(addedEvent.TotalSeats, getTargetEvent.TotalSeats);
        Assert.Equal(addedEvent.AvailableSeats, getTargetEvent.AvailableSeats);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new EventRepository(context);

        // Act & Assert
        var getTargetEvent = await repository.GetByIdAsync(1);

        Assert.Null(getTargetEvent);
    }

    [Fact]
    public async Task DeleteAsync_ExistingEvent_RemovesEventFromDatabase()
    {
        // Arrange
        var addedEvent = new Event
        {
            Title = "Title1",
            Description = "Description1",
            StartAt = DateTime.UtcNow.AddDays(-5),
            EndAt = DateTime.UtcNow.AddDays(3),
            TotalSeats = 10,
            AvailableSeats = 10
        };

        await using (var context = CreateContext())
        {
            var repository = new EventRepository(context);
            await repository.AddAsync(addedEvent);
            await repository.SaveChangesAsync();
        }

        // Act
        await using (var actContext = CreateContext())
        {
            var actRepository = new EventRepository(actContext);

            var eventToDelete = await actRepository.GetByIdAsync(addedEvent.Id);
            Assert.NotNull(eventToDelete);

            await actRepository.DeleteAsync(eventToDelete);
            await actRepository.SaveChangesAsync();
        }

        // Assert
        await using var verifyContext = CreateContext();
        var verifyRepository = new EventRepository(verifyContext);

        var targetEvent = await verifyRepository.GetByIdAsync(addedEvent.Id);

        Assert.Null(targetEvent);
    }

    [Fact]
    public async Task FilterEvents_ByTitle_ReturnsCorrectEvents()
    {
        // Arrange
        await using var context = CreateContext();
        const string targetTitle = "Title1";

        var repository = new EventRepository(context);
        var addedEvent1 = new Event
        {
            Title = targetTitle,
            Description = "Description1",
            StartAt = DateTime.UtcNow.AddDays(-5),
            EndAt = DateTime.UtcNow.AddDays(3),
            TotalSeats = 10,
            AvailableSeats = 10
        };
        var addedEvent2 = new Event
        {
            Title = "Title2",
            Description = "Description2",
            StartAt = DateTime.UtcNow.AddDays(-5),
            EndAt = DateTime.UtcNow.AddDays(3),
            TotalSeats = 10,
            AvailableSeats = 10
        };

        // Act
        await repository.AddAsync(addedEvent1);
        await repository.AddAsync(addedEvent2);
        await repository.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateContext();
        var verifyRepository = new EventRepository(verifyContext);

        var getTargetEventQuery = verifyRepository.FilterEvents(title: targetTitle);
        var result = await getTargetEventQuery.ToListAsync();

        Assert.NotNull(result);
        Assert.Single(result);

        var matchedEvent = result.First();

        Assert.Equal(targetTitle, matchedEvent.Title);
        Assert.Equal(addedEvent1.Description, matchedEvent.Description);
    }

    [Fact]
    public async Task FilterEvents_ByDateRange_ReturnsCorrectEvents()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var filterFrom = baseTime.AddDays(2);
        var filterTo = baseTime.AddDays(5);

        var targetEvent = new Event
        {
            Title = "Target Event",
            Description = "In range",
            StartAt = baseTime.AddDays(3),
            EndAt = baseTime.AddDays(4),
            TotalSeats = 10, AvailableSeats = 10
        };
        var tooEarlyEvent = new Event
        {
            Title = "Too Early Event",
            Description = "Starts too soon",
            StartAt = baseTime.AddDays(1),
            EndAt = baseTime.AddDays(4),
            TotalSeats = 10, AvailableSeats = 10
        };
        var tooLateEvent = new Event
        {
            Title = "Too Late Event",
            Description = "Ends too late",
            StartAt = baseTime.AddDays(3),
            EndAt = baseTime.AddDays(6),
            TotalSeats = 10, AvailableSeats = 10
        };

        await using (var context = CreateContext())
        {
            var repository = new EventRepository(context);
            await repository.AddAsync(targetEvent);
            await repository.AddAsync(tooEarlyEvent);
            await repository.AddAsync(tooLateEvent);
            await repository.SaveChangesAsync();
        }

        // Act
        await using var verifyContext = CreateContext();
        var verifyRepository = new EventRepository(verifyContext);

        var query = verifyRepository.FilterEvents(from: filterFrom, to: filterTo);
        var result = await query.ToListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Target Event", result[0].Title);
    }

    [Fact]
    public async Task FilterEvents_CombinedFilters_ReturnsCorrectEvents()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        const string partialTitle = "Event";

        var matchedEvent = new Event
        {
            Title = "title1Event",
            Description = "Matched",
            StartAt = baseTime.AddDays(1),
            EndAt = baseTime.AddDays(3),
            TotalSeats = 10, AvailableSeats = 10
        };
        var wrongTitleEvent = new Event
        {
            Title = "title2",
            Description = "Wrong title",
            StartAt = baseTime.AddDays(1),
            EndAt = baseTime.AddDays(3),
            TotalSeats = 10, AvailableSeats = 10
        };
        var wrongDateEvent = new Event
        {
            Title = "title3Event",
            Description = "Wrong date",
            StartAt = baseTime.AddDays(10),
            EndAt = baseTime.AddDays(12),
            TotalSeats = 10, AvailableSeats = 10
        };

        await using (var context = CreateContext())
        {
            var repository = new EventRepository(context);
            await repository.AddAsync(matchedEvent);
            await repository.AddAsync(wrongTitleEvent);
            await repository.AddAsync(wrongDateEvent);
            await repository.SaveChangesAsync();
        }

        // Act
        await using var verifyContext = CreateContext();
        var verifyRepository = new EventRepository(verifyContext);

        var query = verifyRepository.FilterEvents(
            title: partialTitle,
            from: baseTime,
            to: baseTime.AddDays(5));

        var result = await query.ToListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("title1Event", result[0].Title);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesMultipleEntities_SavesAll()
    {
        // Arrange
        var event1 = new Event
        {
            Title = "Title1", Description = "Description1", StartAt = DateTime.UtcNow.AddDays(-5),
            EndAt = DateTime.UtcNow.AddDays(3), TotalSeats = 10, AvailableSeats = 10
        };
        var event2 = new Event
        {
            Title = "Title2", Description = "Description2",
            StartAt = new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2025, 12, 30, 0, 0, 0, DateTimeKind.Utc), TotalSeats = 10, AvailableSeats = 10
        };
        var event3 = new Event
        {
            Title = "Title3", Description = "Description3", StartAt = DateTime.UtcNow.AddDays(-8),
            EndAt = DateTime.UtcNow.AddDays(5), TotalSeats = 10, AvailableSeats = 10
        };

        // Act
        await using (var context = CreateContext())
        {
            var repository = new EventRepository(context);

            await repository.AddAsync(event1);
            await repository.AddAsync(event2);
            await repository.AddAsync(event3);

            await repository.SaveChangesAsync();
        }

        // Assert
        await using var verifyContext = CreateContext();
        var verifyRepository = new EventRepository(verifyContext);

        var allSavedEvents = await verifyRepository.Query().ToListAsync();

        Assert.Equal(3, allSavedEvents.Count);

        Assert.Contains(allSavedEvents, e => e.Title == "Title1");
        Assert.Contains(allSavedEvents, e => e.Title == "Title2");
        Assert.Contains(allSavedEvents, e => e.Title == "Title3");
    }
}