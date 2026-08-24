using AutoMapper;
using EventOrchestrationService.Contracts.DTOs;
using EventService.Application.Interfaces;
using EventService.Application.Mappings;
using EventService.Application.Validators;
using EventService.Domain.Entities;
using EventService.Infrastructure.Data;
using EventService.Infrastructure.Data.Repositories;
using EventService.Infrastructure.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventService.UnitTests;

public class EventServiceCachingTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Application.Services.EventService _eventService;
    private readonly Mock<ICacheService> _cacheMock;

    public EventServiceCachingTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        var eventRepository = new EventRepository(_context);
        var unitOfWork = new UnitOfWork(_context);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { cfg.AddProfile<EventMappingProfile>(); });
        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var createEventValidator = new CreateEventContractDtoValidator();
        var updateEventValidator = new UpdateEventContractDtoValidator();

        _cacheMock = new Mock<ICacheService>();
        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _eventService = new Application.Services.EventService(
            createEventValidator,
            updateEventValidator,
            eventRepository,
            mapper,
            _cacheMock.Object,
            unitOfWork
        );
    }

    public void Dispose()
    {
        _context?.Database.CloseConnection();
        _context?.Dispose();
    }

    /// <summary>
    /// При попадании в кеш репозиторий не вызывается
    /// </summary>
    [Fact]
    public async Task GetEventByIdAsync_WhenInCache_DoesNotCallRepository()
    {
        // Arrange
        const int eventId = 1;
        var cacheKey = $"event:{eventId}";
        var cachedEvent = new EventContractDto { Id = eventId, Title = "Cached Event" };

        _cacheMock
            .Setup(c => c.GetAsync<EventContractDto>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedEvent);

        // Act
        var result = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cachedEvent.Id, result.Id);
        Assert.Equal(cachedEvent.Title, result.Title);
    }

    /// <summary>
    /// При промахе данные берутся из репозитория и сохраняются в кеш
    /// </summary>
    [Fact]
    public async Task GetEventByIdAsync_WhenNotInCache_CallsRepositoryAndSavesToCache()
    {
        // Arrange
        const int eventId = 1;
        var cacheKey = $"event:{eventId}";

        var eventEntity = new Event(
            title: "Test Event",
            description: "Test Description",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10
        );
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        _cacheMock
            .Setup(c => c.GetAsync<EventContractDto>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventContractDto?)null);

        // Act
        var result = await _eventService.GetEventByIdAsync(eventEntity.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventEntity.Id, result.Id);
        Assert.Equal(eventEntity.Title, result.Title);

        _cacheMock.Verify(
            c => c.SetAsync(
                cacheKey,
                It.Is<EventContractDto>(d => d.Id == eventEntity.Id),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    /// <summary>
    /// При обновлении события кеш инвалидируется
    /// </summary>
    [Fact]
    public async Task UpdateEventAsync_InvalidatesCache()
    {
        // Arrange
        var updateDto = new UpdateEventContractDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 20
        };

        var existingEvent = new Event(
            title: "Old Title",
            description: "Old Description",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10
        );
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        var cacheKey = $"event:{existingEvent.Id}";

        // Act
        await _eventService.UpdateEventAsync(existingEvent.Id, updateDto, CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            c => c.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    /// <summary>
    /// При удалении события кеш инвалидируется
    /// </summary>
    [Fact]
    public async Task DeleteEventAsync_InvalidatesCache()
    {
        // Arrange
        var existingEvent = new Event(
            title: "Test Event",
            description: "Test Description",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10
        );
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        var cacheKey = $"event:{existingEvent.Id}";

        // Act
        await _eventService.DeleteEventAsync(existingEvent.Id, CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            c => c.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()),
            Times.Once
        );

        var deletedEvent = await _context.Events.FindAsync(existingEvent.Id);
        Assert.Null(deletedEvent);
    }

    /// <summary>
    /// GetTop10Async кеширует результат и не вызывает репозиторий при попадании
    /// </summary>
    [Fact]
    public async Task GetTop10Async_WhenInCache_DoesNotCallRepository()
    {
        // Arrange
        var cacheKey = "event:'top10'";
        var cachedEvents = new List<EventContractDto>
        {
            new() { Id = 1, Title = "Event 1" },
            new() { Id = 2, Title = "Event 2" }
        };

        _cacheMock
            .Setup(c => c.GetAsync<List<EventContractDto>>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedEvents);

        // Act
        var result = await _eventService.GetTop10Async(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(cachedEvents[0].Id, result[0].Id);
        Assert.Equal(cachedEvents[0].Title, result[0].Title);
        Assert.Equal(cachedEvents[1].Id, result[1].Id);
        Assert.Equal(cachedEvents[1].Title, result[1].Title);
    }
}