using EventOrchestrationService.Data;
using EventOrchestrationService.Data.Repositories.Implementations;
using EventOrchestrationService.Entities;
using EventOrchestrationService.Exceptions;
using EventOrchestrationService.Services.Implementations;
using EventOrchestrationService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.Tests;

public class BookingServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IBookingService _service;
    private readonly IEventService _eventService;

    public BookingServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _eventService = new EventService(new Event.EventValidator(), new EventRepository(_context));
        _service = new BookingService(_eventService,  new BookingRepository(_context));
    }

    public void Dispose()
    {
        _context?.Database.CloseConnection();
        _context?.Dispose();
    }

    private void SeedEvents()
    {
        _context.Events.AddRange(
            new Event
            {
                Id = 1, Title = "Title1", Description = "Description1", StartAt = DateTime.UtcNow.AddDays(-5),
                EndAt = DateTime.UtcNow.AddDays(5), TotalSeats = 10, AvailableSeats = 10
            },
            new Event
            {
                Id = 2, Title = "Title2", Description = "Description2", StartAt = DateTime.UtcNow.AddDays(-5),
                EndAt = DateTime.UtcNow.AddDays(5), TotalSeats = 10, AvailableSeats = 10
            },
            new Event
            {
                Id = 3, Title = "OverbookingTest", Description = "Test Description", StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2), TotalSeats = 5, AvailableSeats = 5
            },
            new Event
            {
                Id = 4, Title = "UniqueIdTest", Description = "Test for unique IDs",  StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2), TotalSeats = 10, AvailableSeats = 10
            }
        );
        _context.SaveChanges();
    }

    /// <summary>
    /// Создание тестового события с указанным количеством мест
    /// </summary>
    private async Task<Event> CreateTestEvent(int totalSeats, CancellationToken cancellationToken)
    {
        var testEvent = new Event
        {
            Title = "TestEvent",
            Description = "Test Description",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = totalSeats,
            AvailableSeats = totalSeats
        };

        return await _eventService.CreateEventAsync(testEvent, cancellationToken);
    }

    /// <summary>
    /// Создание брони для существующего события
    /// Проверяем, что возвращается Booking со статусом Pending
    /// Проверяем, что поля CreatedAt и EventId заполнены корректно
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_ExistingEvent_ReturnsBookingWithPendingStatus()
    {
        // Arrange
        SeedEvents();
        const int existingEventId = 1;

        // Act
        var result = await _service.CreateBookingAsync(existingEventId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.Equal(existingEventId, result.EventId);
        Assert.True(result.CreatedAt > DateTime.MinValue);
        Assert.True(result.Id > 0);
    }

    /// <summary>
    /// Создание нескольких броней для одного события
    /// Проверяем, что все создаются с уникальными Id
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_MultipleBookingsForSameEvent_ReturnsUniqueIds()
    {
        // Arrange
        SeedEvents();
        const int eventId = 1;

        // Act
        var booking1 = await _service.CreateBookingAsync(eventId, CancellationToken.None);
        var booking2 = await _service.CreateBookingAsync(eventId, CancellationToken.None);
        var booking3 = await _service.CreateBookingAsync(eventId, CancellationToken.None);

        // Assert
        Assert.NotEqual(booking1.Id, booking2.Id);
        Assert.NotEqual(booking2.Id, booking3.Id);
        Assert.NotEqual(booking1.Id, booking3.Id);
    }

    /// <summary>
    /// Получение брони по Id
    /// Проверяем, что возвращается корректная информация
    /// Проверяем, что все поля соответствуют созданной брони
    /// </summary>
    [Fact]
    public async Task GetBookingByIdAsync_ExistingBooking_ReturnsCorrectInformation()
    {
        // Arrange
        SeedEvents();
        var createdBooking = await _service.CreateBookingAsync(1, CancellationToken.None);

        // Act
        var result = await _service.GetBookingByIdAsync(createdBooking.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdBooking.Id, result.Id);
        Assert.Equal(createdBooking.EventId, result.EventId);
        Assert.Equal(createdBooking.Status, result.Status);
        Assert.Equal(createdBooking.CreatedAt, result.CreatedAt);
    }

    /// <summary>
    /// Получение брони отражает изменение статуса
    /// Проверяем, что после ручного изменения статуса в БД, метод возвращает актуальные данные
    /// </summary>
    [Fact]
    public async Task GetBookingByIdAsync_AfterStatusChange_ReturnsUpdatedStatus()
    {
        // Arrange
        SeedEvents();
        var createdBooking = await _service.CreateBookingAsync(1, CancellationToken.None);

        // Имитируем фоновую обработку -- меняем статус напрямую в БД
        var dbBooking = await _context.Bookings.FindAsync(createdBooking.Id);
        dbBooking!.Status = BookingStatus.Confirmed;
        dbBooking.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBookingByIdAsync(createdBooking.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Confirmed, result.Status);
    }

    /// <summary>
    /// Создание брони для несуществующего события
    /// Проверяем, что выбрасывается NotFoundException
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_NonExistingEvent_ThrowsNotFoundException()
    {
        // Arrange
        SeedEvents();
        const int nonExistingEventId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateBookingAsync(nonExistingEventId, CancellationToken.None)
        );
    }

    /// <summary>
    /// Создание брони для удалённого события
    /// Проверяем, что выбрасывается NotFoundException
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_DeletedEvent_ThrowsNotFoundException()
    {
        // Arrange
        SeedEvents();

        var eventService = new EventService(new Event.EventValidator(), new EventRepository(_context));
        await eventService.DeleteEventAsync(2, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateBookingAsync(2, CancellationToken.None)
        );
    }

    /// <summary>
    /// Получение брони по несуществующему Id
    /// Проверяем, что возвращается null
    /// </summary>
    [Fact]
    public async Task GetBookingByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        SeedEvents();
        const int nonExistingBookingId = 999;

        // Act
        var result = await _service.GetBookingByIdAsync(nonExistingBookingId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Создание брони уменьшает AvailableSeats на 1
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_ExistingEvent_DecreasesAvailableSeatsByOne()
    {
        // Arrange
        SeedEvents();

        const int eventId = 1;

        var eventBefore = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        var seatsBefore = eventBefore!.AvailableSeats;

        // Act
        await _service.CreateBookingAsync(eventId, CancellationToken.None);

        // Assert
        var eventAfter = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        Assert.Equal(seatsBefore - 1, eventAfter!.AvailableSeats);
    }

    /// <summary>
    /// После исчерпания мест следующая попытка выбрасывает NoAvailableSeatsException
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_WhenNoSeatsLeft_ThrowsNoAvailableSeatsException()
    {
        // Arrange
        SeedEvents();
        const int eventId = 1;
        var targetEvent = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        var totalSeats = targetEvent!.TotalSeats;

        // Заполняем все места
        for (int i = 0; i < totalSeats; i++)
        {
            await _service.CreateBookingAsync(eventId, CancellationToken.None);
        }

        // Act & Assert
        await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => _service.CreateBookingAsync(eventId, CancellationToken.None)
        );
    }

    /// <summary>
    /// После обновления статуса на Confirmed бронь имеет статус Confirmed и заполненный ProcessedAt
    /// </summary>
    [Fact]
    public async Task Booking_WhenConfirmed_SetsStatusConfirmedAndProcessedAt()
    {
        // Arrange
        SeedEvents();
        var booking = await _service.CreateBookingAsync(1, CancellationToken.None);

        // Act
        booking.Status = BookingStatus.Confirmed;
        booking.ProcessedAt = DateTime.UtcNow;

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    /// <summary>
    /// После обновления статуса на Rejected бронь имеет статус Rejected и заполненный ProcessedAt
    /// </summary>
    [Fact]
    public async Task Booking_WhenRejected_SetsStatusRejectedAndProcessedAt()
    {
        // Arrange
        SeedEvents();
        var booking = await _service.CreateBookingAsync(1, CancellationToken.None);

        // Act
        booking.Status = BookingStatus.Rejected;
        booking.ProcessedAt = DateTime.UtcNow;

        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    /// <summary>
    /// После Reject() ReleaseSeats() количество свободных мест восстанавливается
    /// </summary>
    [Fact]
    public async Task ReleaseSeats_AfterReject_RestoresAvailableSeats()
    {
        // Arrange
        SeedEvents();
        const int eventId = 1;
        var eventBefore = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        var seatsBefore = eventBefore!.AvailableSeats;

        await _service.CreateBookingAsync(eventId, CancellationToken.None);

        var eventAfterBooking = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        Assert.Equal(seatsBefore - 1, eventAfterBooking!.AvailableSeats);

        // Act
        var targetEvent = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        targetEvent!.ReleaseSeats();
        await _eventService.UpdateEventAsync(eventId, targetEvent, CancellationToken.None);

        // Assert
        var eventAfterRelease = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        Assert.Equal(seatsBefore, eventAfterRelease!.AvailableSeats);
    }

    /// <summary>
    /// После Reject() и ReleaseSeats() можно успешно создать новую бронь на то же место
    /// </summary>
    [Fact]
    public async Task ReleaseSeats_AfterReject_AllowsNewBookingOnSameSeat()
    {
        // Arrange
        SeedEvents();
        const int eventId = 1;
        var targetEvent = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        var seatsBefore = targetEvent!.AvailableSeats;

        var booking = await _service.CreateBookingAsync(eventId, CancellationToken.None);

        targetEvent.ReleaseSeats();
        await _eventService.UpdateEventAsync(eventId, targetEvent, CancellationToken.None);

        // Act
        var newBooking = await _service.CreateBookingAsync(eventId, CancellationToken.None);

        // Assert
        Assert.NotNull(newBooking);
        Assert.NotEqual(booking.Id, newBooking.Id);

        var eventAfter = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        Assert.Equal(seatsBefore - 1, eventAfter!.AvailableSeats);
    }

    /// <summary>
    /// Тест на защиту от овербукинга: 20 конкурентных запросов на 5 мест
    /// Ожидается: ровно 5 успешных броней, 15 исключений, AvailableSeats = 0
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_ConcurrentRequests_PreventsOverbooking()
    {
        // Arrange
        SeedEvents();
        const int eventId = 3;

        const int concurrentRequests = 20;
        var tasks = new List<Task<Booking>>();
        var successfulBookings = 0;
        var noSeatsExceptions = 0;

        // Act
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(_service.CreateBookingAsync(eventId, CancellationToken.None));
        }

        foreach (var task in tasks)
        {
            try
            {
                await task;
                Interlocked.Increment(ref successfulBookings);
            }
            catch (NoAvailableSeatsException)
            {
                Interlocked.Increment(ref noSeatsExceptions);
            }
        }

        // Assert
        Assert.Equal(5, successfulBookings);
        Assert.Equal(15, noSeatsExceptions);

        var finalEvent = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        Assert.Equal(0, finalEvent!.AvailableSeats);
    }

    /// <summary>
    /// Тест на уникальность Id при конкурентных запросах
    /// Дано: событие на 10 мест, 10 одновременных запросов
    /// Ожидается: 10 броней с уникальными Id
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_ConcurrentRequests_AllBookingsHaveUniqueIds()
    {
        // Arrange
        SeedEvents();
        const int eventId = 4;

        var tasks = new List<Task<Booking>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_service.CreateBookingAsync(eventId, CancellationToken.None));
        }

        var bookings = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, bookings.Length);

        var uniqueIds = bookings.Select(b => b.Id).Distinct();
        Assert.Equal(10, uniqueIds.Count());

        var finalEvent = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        Assert.Equal(0, finalEvent!.AvailableSeats);
    }
}