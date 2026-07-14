using EventOrchestrationService.Domain.Entities;
using EventOrchestrationService.Domain.Exceptions;
using EventOrchestrationService.Infrastructure.Data.Repositories;
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
        var addedEvent = new Event(
            title: "Title1",
            description: "Description1",
            startAt: DateTime.UtcNow.AddDays(-5),
            endAt: DateTime.UtcNow.AddDays(3),
            totalSeats: 10
        );

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
        var addedEvent = new Event(
            title: "Title1",
            description: "Description1",
            startAt: DateTime.UtcNow.AddDays(-5),
            endAt: DateTime.UtcNow.AddDays(3),
            totalSeats: 10
        );

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
        var addedEvent = new Event(
            title: "Title1",
            description: "Description1",
            startAt: DateTime.UtcNow.AddDays(-5),
            endAt: DateTime.UtcNow.AddDays(3),
            totalSeats: 10
        );

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
        var addedEvent1 = new Event(
            title: targetTitle,
            description: "Description1",
            startAt: DateTime.UtcNow.AddDays(-5),
            endAt: DateTime.UtcNow.AddDays(3),
            totalSeats: 10
        );

        var addedEvent2 = new Event(
            title: "Title2",
            description: "Description2",
            startAt: DateTime.UtcNow.AddDays(-5),
            endAt: DateTime.UtcNow.AddDays(3),
            totalSeats: 10
        );

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

        var targetEvent = new Event(
            title: "Target Event",
            description: "In range",
            startAt: baseTime.AddDays(3),
            endAt: baseTime.AddDays(4),
            totalSeats: 10
        );

        var tooEarlyEvent = new Event(
            title: "Too Early Event",
            description: "Starts too soon",
            startAt: baseTime.AddDays(1),
            endAt: baseTime.AddDays(4),
            totalSeats: 10
        );

        var tooLateEvent = new Event(
            title: "Too Late Event",
            description: "Ends too late",
            startAt: baseTime.AddDays(3),
            endAt: baseTime.AddDays(6),
            totalSeats: 10
        );

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

        var matchedEvent = new Event(
            title: "title1Event",
            description: "Matched",
            startAt: baseTime.AddDays(1),
            endAt: baseTime.AddDays(3),
            totalSeats: 10
        );

        var wrongTitleEvent = new Event(
            title: "title2",
            description: "Wrong title",
            startAt: baseTime.AddDays(1),
            endAt: baseTime.AddDays(3),
            totalSeats: 10
        );

        var wrongDateEvent = new Event(
            title: "title3Event",
            description: "Wrong date",
            startAt: baseTime.AddDays(10),
            endAt: baseTime.AddDays(12),
            totalSeats: 10
        );

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
        var event1 = new Event(
            title: "Title1",
            description: "Description1",
            startAt: DateTime.UtcNow.AddDays(-5),
            endAt: DateTime.UtcNow.AddDays(3),
            totalSeats: 10
        );

        var event2 = new Event(
            title: "Title2",
            description: "Description2",
            startAt: DateTime.UtcNow.AddDays(-5),
            endAt: DateTime.UtcNow.AddDays(3),
            totalSeats: 10
        );

        var event3 = new Event(
            title: "Title3",
            description: "Description3",
            startAt: DateTime.UtcNow.AddDays(-5),
            endAt: DateTime.UtcNow.AddDays(3),
            totalSeats: 10
        );

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

    [Fact]
    public async Task UpdateAsync_ValidEvent_UpdatesEventInDatabase()
    {
        // Arrange
        var addedEvent = new Event(
            title: "Title1",
            description: "Description1",
            startAt: DateTime.UtcNow.AddDays(-5),
            endAt: DateTime.UtcNow.AddDays(3),
            totalSeats: 10
        );

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
            var eventToUpdate = await actRepository.GetByIdAsync(addedEvent.Id);

            eventToUpdate.Update(
                title: "New Title",
                description: "New Description",
                startAt: DateTime.UtcNow.AddDays(1),
                endAt: DateTime.UtcNow.AddDays(5),
                totalSeats: 20
            );

            await actRepository.SaveChangesAsync();
        }

        // Assert
        await using var verifyContext = CreateContext();
        var saved = await verifyContext.Events.FirstOrDefaultAsync(e => e.Id == addedEvent.Id);

        Assert.NotNull(saved);
        Assert.Equal("New Title", saved.Title);
        Assert.Equal("New Description", saved.Description);
        Assert.Equal(20, saved.TotalSeats);
    }

    [Fact]
    public async Task FilterEvents_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var events = Enumerable.Range(1, 25).Select(i => new Event(
            title: $"Event {i}",
            description: $"Description {i}",
            startAt: DateTime.UtcNow.AddDays(i),
            endAt: DateTime.UtcNow.AddDays(i + 1),
            totalSeats: 10
        )).ToArray();

        await using (var context = CreateContext())
        {
            var repository = new EventRepository(context);
            foreach (var ev in events)
            {
                await repository.AddAsync(ev);
            }

            await repository.SaveChangesAsync();
        }

        // Act
        await using var actContext = CreateContext();
        var actRepository = new EventRepository(actContext);
        var (items, totalCount) = await actRepository.GetPagedEventsAsync(page: 1, pageSize: 10);

        // Assert
        Assert.Equal(10, items.Count);
        Assert.Equal(25, totalCount);
        Assert.Equal("Event 1", items.First().Title);
    }

    [Fact]
    public async Task UpdateAsync_ConcurrentUpdate_ThrowsConcurrencyException()
    {
        // Arrange
        var addedEvent = new Event(
            title: "Title1",
            description: "Description1",
            startAt: DateTime.UtcNow.AddDays(-5),
            endAt: DateTime.UtcNow.AddDays(3),
            totalSeats: 10
        );

        await using (var context = CreateContext())
        {
            var repository = new EventRepository(context);
            await repository.AddAsync(addedEvent);
            await repository.SaveChangesAsync();
        }

        // Act
        await using var context1 = CreateContext();
        await using var context2 = CreateContext();

        var repo1 = new EventRepository(context1);
        var repo2 = new EventRepository(context2);

        var event1 = await repo1.GetByIdAsync(addedEvent.Id);
        var event2 = await repo2.GetByIdAsync(addedEvent.Id);

        event1.Update("New Title1", null, null, null, null);
        var versionBefore = event1.RowVersion;
        await repo1.SaveChangesAsync();
        var versionAfter = (await repo1.GetByIdAsync(addedEvent.Id)).RowVersion;
        Assert.NotEqual(versionBefore, versionAfter); 

        event2.Update("New Title2", null, null, null, null);
        await Assert.ThrowsAsync<ConcurrencyException>(() => repo2.SaveChangesAsync());
    }
}