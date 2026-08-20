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

public class EventServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IEventService _service;

    public EventServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        var eventRepository = new EventRepository(_context);
        var createEventValidator = new CreateEventContractDtoValidator();
        var updateEventValidator = new UpdateEventContractDtoValidator();

        var unitOfWork = new UnitOfWork(_context);

        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<EventMappingProfile>();
        });

        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var cacheMock = new Mock<ICacheService>();
        var transactionWrapperMock = new Mock<ITransactionWrapper>();

        transactionWrapperMock
            .Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);


        _service = new Application.Services.EventService(
            createEventValidator,
            updateEventValidator,
            eventRepository,
            mapper,
            cacheMock.Object,
            unitOfWork
        );
    }

    public void Dispose()
    {
        _context?.Database.CloseConnection();
        _context?.Dispose();
    }

    private void SeedDatabase()
    {
        var baseTime = DateTime.UtcNow;

        _context.Events.AddRange(
            new Event(
                title: "Title1",
                description: "Description1",
                startAt: baseTime.AddDays(-5),
                endAt: baseTime.AddDays(3),
                totalSeats: 10
            ),
            new Event(
                title: "Title2",
                description: "Description2",
                startAt: new DateTime(2025, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2026, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                totalSeats: 10
            ),
            new Event(
                title: "Title3",
                description: "Description3",
                startAt: baseTime.AddDays(-8),
                endAt: baseTime.AddDays(5),
                totalSeats: 10
            ),
            new Event(
                title: "ABC_Title4",
                description: "Description4",
                startAt: baseTime.AddDays(-8),
                endAt: baseTime.AddDays(5),
                totalSeats: 10
            ),
            new Event(
                title: "abc_Title5",
                description: "Description5",
                startAt: new DateTime(2055, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                endAt: new DateTime(2056, 6, 30, 0, 0, 0, DateTimeKind.Utc),
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
            )
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateEventAsync_WithValidData_Success()
    {
        // Arrange
        var createEventDto = new CreateEventContractDto
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2100, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            TotalSeats = 10
        };

        // Act
        var result = await _service.CreateEventAsync(createEventDto, CancellationToken.None);

        // Assert
        Assert.Equal(createEventDto.Title, result.Title);
        Assert.Equal(createEventDto.Description, result.Description);
        Assert.Equal(createEventDto.StartAt, result.StartAt);
        Assert.Equal(createEventDto.EndAt, result.EndAt);
    }

    [Fact]
    public async Task CreateEventAsync_WithInvalidTotalSeats_ThrowsValidationException()
    {
        // Arrange
        var createEventDto = new CreateEventContractDto
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2100, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            TotalSeats = -10
        };

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () =>
        {
            await _service.CreateEventAsync(createEventDto, CancellationToken.None);
        });
    }

    [Fact]
    public async Task GetEvents_WhenEventsExist_ReturnsPaginatedResult()
    {
        // Arrange
        SeedDatabase();

        const int defaultPage = 1;
        const int targetPage = 3;

        // Act
        var getDefaultEventsResult = await _service.GetEventsAsync();
        var getEventsResultWithPageParameter = await _service.GetEventsAsync(page: 3);

        // Assert
        Assert.NotNull(getDefaultEventsResult);
        Assert.NotNull(getEventsResultWithPageParameter);

        Assert.Equal(defaultPage, getDefaultEventsResult.Page);
        Assert.Equal(targetPage, getEventsResultWithPageParameter.Page);
    }

    [Fact]
    public async Task GetEventById_WithValidId_ReturnsEvent()
    {
        // Arrange
        var createEventDto = new CreateEventContractDto
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2100, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            TotalSeats = 10
        };

        var created = await _service.CreateEventAsync(createEventDto, CancellationToken.None);

        // Act
        var result = await _service.GetEventByIdAsync(created.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.Title, result.Title);
        Assert.Equal(created.Description, result.Description);
        Assert.Equal(created.StartAt, result.StartAt);
        Assert.Equal(created.EndAt, result.EndAt);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedEvent()
    {
        // Arrange
        var createEventDto = new CreateEventContractDto
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2100, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            TotalSeats = 10
        };

        var created = await _service.CreateEventAsync(createEventDto, CancellationToken.None);

        var updateDto = new UpdateEventContractDto
        {
            Title = "updatedTitle2",
            Description = "updatedDescription2",
            StartAt = new DateTime(2029, 1, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2029, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            TotalSeats = 10
        };

        // Act
        var updateResult = await _service.UpdateEventAsync(created.Id, updateDto, CancellationToken.None);

        // Assert
        Assert.NotNull(updateResult);
        Assert.Equal(updateDto.Title, updateResult.Title);
        Assert.Equal(updateDto.Description, updateResult.Description);
        Assert.Equal(updateDto.StartAt, updateResult.StartAt);
        Assert.Equal(updateDto.EndAt, updateResult.EndAt);
    }

    [Fact]
    public async Task Delete_WithValidId_DeleteSuccess()
    {
        var createEventDto = new CreateEventContractDto
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2100, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            TotalSeats = 10
        };

        var created = await _service.CreateEventAsync(createEventDto, CancellationToken.None);

        // Act
        var getEventBeforeDelete = await _service.GetEventByIdAsync(created.Id, CancellationToken.None);
        var deleteResult = await _service.DeleteEventAsync(created.Id, CancellationToken.None);
        var getEventAfterDeleting = await _service.GetEventByIdAsync(created.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(getEventBeforeDelete);
        Assert.True(deleteResult);
        Assert.Null(getEventAfterDeleting);
    }

    [Fact]
    public async Task GetEvents_WithTitleFilter_ReturnsMatchingEvents()
    {
        // Arrange
        SeedDatabase();
        var expectedIds = new[] { 4, 5, 6 };

        // Act
        var getResult = await _service.GetEventsAsync(title: "abc");

        // Assert
        Assert.NotNull(getResult);
        Assert.Equal(expectedIds.Length, getResult.Items.Count());
        Assert.Equal(expectedIds, getResult.Items.Select(e => e.Id));
    }

    [Fact]
    public async Task GetEvents_WithDateRange_ReturnsEventsWithinRange()
    {
        // Arrange
        SeedDatabase();
        var expectedIdsWithFromFilter = new[] { 5, 6, 7 };
        var expectedIdsWithToFilter = new[] { 1, 2, 3, 4, 9 };

        // Act
        var getResultWithFromFilter = await _service.GetEventsAsync(from: new DateTime(2055, 1, 1));
        var getResultWithToFilter = await _service.GetEventsAsync(to: new DateTime(2030, 12, 31));

        // Assert
        Assert.Equal(expectedIdsWithFromFilter, getResultWithFromFilter.Items.Select(e => e.Id));
        Assert.Equal(expectedIdsWithToFilter, getResultWithToFilter.Items.Select(e => e.Id));
    }

    [Fact]
    public async Task GetEvents_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        SeedDatabase();
        const int defaultPage = 1;
        const int targetPage = 3;

        const int defaultPageSize = 10;
        const int targetPageSize = 2;

        var expectedIdsForDefaultParameters = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var expectedIdsForTargetPageSizeParameters = new[] { 1, 2 };
        var expectedIdsForPageAndPageSizeParameter = new[] { 5, 6 };

        // Act
        var getDefaultEventsResult = await _service.GetEventsAsync();

        var getEventsResultWithPageParameter = await _service.GetEventsAsync(page: targetPage);
        var getEventsResultWithPageAndPageSizeParameter =
            await _service.GetEventsAsync(page: targetPage, pageSize: targetPageSize);
        var getEventsResultWithPageSizeParameter = await _service.GetEventsAsync(pageSize: targetPageSize);

        // Assert
        Assert.Equal(defaultPage, getDefaultEventsResult.Page);
        Assert.Equal(9, getDefaultEventsResult.PageSize);
        Assert.Equal(expectedIdsForDefaultParameters, getDefaultEventsResult.Items.Select(e => e.Id));
        Assert.Equal(targetPage, getEventsResultWithPageParameter.Page);
        Assert.Equal(targetPageSize, getEventsResultWithPageSizeParameter.PageSize);
        Assert.Equal(expectedIdsForPageAndPageSizeParameter,
            getEventsResultWithPageAndPageSizeParameter.Items.Select(e => e.Id));
        Assert.Equal(expectedIdsForTargetPageSizeParameters,
            getEventsResultWithPageSizeParameter.Items.Select(e => e.Id));
    }

    [Fact]
    public async Task GetEvents_WithMultipleFilters_ReturnsFilteredEvents()
    {
        // Arrange
        SeedDatabase();
        var expectedIds = new[] { 4 };

        // Act
        var getResult = await _service.GetEventsAsync(
            title: "abc",
            from: new DateTime(2025, 1, 1),
            to: new DateTime(2029, 1, 1)
        );

        // Assert
        Assert.Equal(expectedIds, getResult.Items.Select(e => e.Id));
    }

    [Fact]
    public async Task GetEventById_WithNonExistentId_ReturnNull()
    {
        // Arrange
        const int wrongId = 100;

        // Act
        var result = await _service.GetEventByIdAsync(wrongId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        const int wrongId = 100;

        var updateDto = new UpdateEventContractDto
        {
            Title = "updatedTitle2",
            Description = "updatedDescription2",
            StartAt = new DateTime(2029, 1, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2029, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            TotalSeats = 10
        };

        // Act
        var result = await _service.UpdateEventAsync(wrongId, updateDto, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateEventAsync_WithValidData_SetsAvailableSeatsEqualToTotalSeats()
    {
        // Arrange
        var createEventDto = new CreateEventContractDto
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2100, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            TotalSeats = 10
        };

        // Act
        var result = await _service.CreateEventAsync(createEventDto, CancellationToken.None);

        // Assert
        Assert.Equal(createEventDto.TotalSeats, result.AvailableSeats);
    }

    [Fact]
    public async Task Update_WithPartialData_UpdatesOnlySpecifiedFields()
    {
        // Arrange
        var createDto = new CreateEventContractDto
        {
            Title = "Original Title",
            Description = "Original Description",
            StartAt = new DateTime(2099, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2100, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            TotalSeats = 10
        };

        var created = await _service.CreateEventAsync(createDto, CancellationToken.None);

        var updateDto = new UpdateEventContractDto
        {
            Title = "New Title"
        };

        // Act
        var updateResult = await _service.UpdateEventAsync(created.Id, updateDto, CancellationToken.None);

        // Assert
        Assert.NotNull(updateResult);
        Assert.Equal("New Title", updateResult.Title);
        Assert.Equal(createDto.Description, updateResult.Description);
        Assert.Equal(createDto.StartAt, updateResult.StartAt);
        Assert.Equal(createDto.EndAt, updateResult.EndAt);
        Assert.Equal(createDto.TotalSeats, updateResult.TotalSeats);
    }

    [Fact]
    public async Task Update_WithInvalidDates_ThrowsValidationException()
    {
        // Arrange
        var createDto = new CreateEventContractDto
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2100, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            TotalSeats = 10
        };

        var created = await _service.CreateEventAsync(createDto, CancellationToken.None);

        var updateDto = new UpdateEventContractDto
        {
            Title = "New Title",
            StartAt = new DateTime(2100, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2099, 12, 30, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _service.UpdateEventAsync(created.Id, updateDto, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Update_WithInvalidTotalSeats_ThrowsValidationException()
    {
        // Arrange
        var createDto = new CreateEventContractDto
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2100, 12, 30, 0, 0, 0, DateTimeKind.Utc),
            TotalSeats = 10
        };

        var created = await _service.CreateEventAsync(createDto, CancellationToken.None);

        var updateDto = new UpdateEventContractDto
        {
            Title = "New Title",
            TotalSeats = -5
        };

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _service.UpdateEventAsync(created.Id, updateDto, CancellationToken.None)
        );
    }
}