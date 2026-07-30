using EventOrchestrationService.Application.DTOs;
using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Application.Services;
using EventOrchestrationService.Application.Settings;
using EventOrchestrationService.Application.Validators;
using EventOrchestrationService.Domain.Entities;
using EventOrchestrationService.Domain.Enums;
using EventOrchestrationService.Domain.Exceptions;
using EventOrchestrationService.Infrastructure.Data;
using EventOrchestrationService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventOrchestrationService.Tests;

public class BookingServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IBookingService _service;
    private readonly IEventService _eventService;
    private readonly IBookingRepository _bookingRepository;

    public BookingServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        var eventRepository = new EventRepository(_context);
        var bookingRepository = new BookingRepository(_context);
        _bookingRepository = bookingRepository;

        var createEventValidator = new CreateEventDtoValidator();
        var updateEventValidator = new UpdateEventDtoValidator();

        var bookingSettings = new BookingSettings
        {
            MaxBookingsPerUser = 100
        };

        var optionsWrapper = Options.Create(bookingSettings);

        // Создаем сервисы
        _eventService = new EventService(
            createEventValidator,
            updateEventValidator,
            eventRepository
        );

        _service = new BookingService(
            _eventService,
            bookingRepository,
            optionsWrapper
        );
    }

    private BookingService CreateBookingService(int maxBookingsPerUser)
    {
        var bookingSettings = new BookingSettings
        {
            MaxBookingsPerUser = maxBookingsPerUser
        };
        var optionsWrapper = Options.Create(bookingSettings);

        return new BookingService(
            _eventService,
            _bookingRepository,
            optionsWrapper
        );
    }

    public void Dispose()
    {
        _context?.Database.CloseConnection();
        _context?.Dispose();
    }

    private void SeedEvents()
    {
        var baseTime = DateTime.UtcNow;

        _context.Events.AddRange(
            new Event(
                title: "Title1",
                description: "Description1",
                startAt: baseTime.AddDays(5),
                endAt: baseTime.AddDays(10),
                totalSeats: 10
            ),
            new Event(
                title: "Title2",
                description: "Description2",
                startAt: baseTime.AddDays(5),
                endAt: baseTime.AddDays(10),
                totalSeats: 10
            ),
            new Event(
                title: "OverbookingTest",
                description: "Test Description",
                startAt: baseTime.AddDays(1),
                endAt: baseTime.AddDays(2),
                totalSeats: 5
            ),
            new Event(
                title: "UniqueIdTest",
                description: "Test for unique IDs",
                startAt: baseTime.AddDays(1),
                endAt: baseTime.AddDays(2),
                totalSeats: 10
            )
        );
        _context.SaveChanges();
    }

    private List<User> SeedUsers()
    {
        var users = new List<User>
        {
            new User(
                login: "TestUser",
                passwordHash: "TestPasswordHash",
                role: Role.User
            ),
            new User(
                login: "TestAdminUser",
                passwordHash: "TestPasswordHash2",
                role: Role.Admin
            )
        };

        _context.Users.AddRange(users);
        _context.SaveChanges();

        return users;
    }

    /// <summary>
    /// Создание тестового события с указанным количеством мест
    /// </summary>
    private async Task<Event> CreateTestEvent(int totalSeats, CancellationToken cancellationToken)
    {
        var baseTime = DateTime.UtcNow;

        var createEventDto = new CreateEventDto
        {
            Title = "TestEvent",
            Description = "Test Description",
            StartAt = baseTime.AddDays(1),
            EndAt = baseTime.AddDays(2),
            TotalSeats = totalSeats
        };

        return await _eventService.CreateEventAsync(createEventDto, cancellationToken);
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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        const int existingEventId = 1;

        // Act
        var result = await _service.CreateBookingAsync(existingEventId, userId, CancellationToken.None);

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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        const int eventId = 1;

        // Act
        var booking1 = await _service.CreateBookingAsync(eventId, userId, CancellationToken.None);
        var booking2 = await _service.CreateBookingAsync(eventId, userId, CancellationToken.None);
        var booking3 = await _service.CreateBookingAsync(eventId, userId, CancellationToken.None);

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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        var createdBooking = await _service.CreateBookingAsync(1, userId, CancellationToken.None);

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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        var createdBooking = await _service.CreateBookingAsync(1, userId, CancellationToken.None);

        // Имитируем фоновую обработку -- меняем статус через доменный метод
        var dbBooking = await _context.Bookings.FindAsync(createdBooking.Id);
        dbBooking!.Confirm();
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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        const int nonExistingEventId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateBookingAsync(nonExistingEventId, userId, CancellationToken.None)
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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;

        // Используем существующий _eventService вместо создания нового
        await _eventService.DeleteEventAsync(2, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateBookingAsync(2, userId, CancellationToken.None)
        );
    }

    /// <summary>
    /// Получение брони по-несуществующему Id
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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;

        const int eventId = 1;

        var eventBefore = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        var seatsBefore = eventBefore!.AvailableSeats;

        // Act
        await _service.CreateBookingAsync(eventId, userId, CancellationToken.None);

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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        const int eventId = 1;
        var targetEvent = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        var totalSeats = targetEvent!.TotalSeats;

        // Заполняем все места
        for (int i = 0; i < totalSeats; i++)
        {
            await _service.CreateBookingAsync(eventId, userId, CancellationToken.None);
        }

        // Act & Assert
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() =>
            _service.CreateBookingAsync(eventId, userId, CancellationToken.None)
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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        var booking = await _service.CreateBookingAsync(1, userId, CancellationToken.None);

        // Act
        booking.Confirm();

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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        var booking = await _service.CreateBookingAsync(1, userId, CancellationToken.None);

        // Act
        booking.Reject();

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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        const int eventId = 1;
        var eventBefore = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        var seatsBefore = eventBefore!.AvailableSeats;

        await _service.CreateBookingAsync(eventId, userId, CancellationToken.None);

        var eventAfterBooking = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        Assert.Equal(seatsBefore - 1, eventAfterBooking!.AvailableSeats);

        // Act
        var targetEvent = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        targetEvent!.ReleaseSeats();

        var updateDto = new UpdateEventDto
        {
            Title = targetEvent.Title,
            Description = targetEvent.Description,
            StartAt = targetEvent.StartAt,
            EndAt = targetEvent.EndAt,
            TotalSeats = targetEvent.TotalSeats
        };
        await _eventService.UpdateEventAsync(eventId, updateDto, CancellationToken.None);

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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        const int eventId = 1;
        var targetEvent = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        var seatsBefore = targetEvent!.AvailableSeats;

        var booking = await _service.CreateBookingAsync(eventId, userId, CancellationToken.None);

        targetEvent.ReleaseSeats();

        var updateDto = new UpdateEventDto
        {
            Title = targetEvent.Title,
            Description = targetEvent.Description,
            StartAt = targetEvent.StartAt,
            EndAt = targetEvent.EndAt,
            TotalSeats = targetEvent.TotalSeats
        };
        await _eventService.UpdateEventAsync(eventId, updateDto, CancellationToken.None);

        // Act
        var newBooking = await _service.CreateBookingAsync(eventId, userId, CancellationToken.None);

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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        const int eventId = 3;

        const int concurrentRequests = 20;
        var tasks = new List<Task<Booking>>();
        var successfulBookings = 0;
        var noSeatsExceptions = 0;

        // Act
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(_service.CreateBookingAsync(eventId, userId, CancellationToken.None));
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
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        const int eventId = 4;

        var tasks = new List<Task<Booking>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_service.CreateBookingAsync(eventId, userId, CancellationToken.None));
        }

        var bookings = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, bookings.Length);

        var uniqueIds = bookings.Select(b => b.Id).Distinct();
        Assert.Equal(10, uniqueIds.Count());

        var finalEvent = await _eventService.GetEventByIdAsync(eventId, CancellationToken.None);
        Assert.Equal(0, finalEvent!.AvailableSeats);
    }

    /// <summary>
    /// Проверка, что повторное подтверждение уже подтвержденной брони выбрасывает исключение
    /// </summary>
    [Fact]
    public void ConfirmAsync_AlreadyConfirmed_ThrowsValidationException()
    {
        // Arrange
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        var booking = new Booking(1, userId, BookingStatus.Pending);
        booking.Confirm();

        // Act & Assert
        Assert.Throws<ValidationException>(() => booking.Confirm());
    }

    /// <summary>
    /// Проверка, что повторное отклонение уже отклоненной брони выбрасывает исключение
    /// </summary>
    [Fact]
    public void RejectAsync_AlreadyRejected_ThrowsValidationException()
    {
        // Arrange
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        var booking = new Booking(1, userId, BookingStatus.Pending);
        booking.Reject();

        // Act & Assert
        Assert.Throws<ValidationException>(() => booking.Reject());
    }

    /// <summary>
    /// Проверка, что Reject() для брони в статусе Pending корректно меняет статус и заполняет ProcessedAt
    /// </summary>
    [Fact]
    public async Task RejectAsync_ValidBooking_UpdatesStatusAndProcessedAt()
    {
        // Arrange
        SeedEvents();
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        var booking = await _service.CreateBookingAsync(1, userId, CancellationToken.None);

        // Act
        booking.Reject();

        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    /// <summary>
    /// Проверка, что бронь не может быть создана с невалидным EventId (меньше или равно 0)
    /// </summary>
    [Fact]
    public void CreateBooking_WithInvalidEventId_ThrowsValidationException()
    {
        var users = SeedUsers();
        var userId = users.First(u => u.Login == "TestUser").Id;
        // Act & Assert
        Assert.Throws<ValidationException>(() => new Booking(0, userId, BookingStatus.Pending));
    }
}