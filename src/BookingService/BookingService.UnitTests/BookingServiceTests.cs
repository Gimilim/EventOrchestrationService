using BookingService.Application.Interfaces;
using BookingService.Application.Settings;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
using BookingService.Domain.Exceptions;
using BookingService.Infrastructure.Data;
using BookingService.Infrastructure.Data.Repositories;
using EventOrchestrationService.Contracts.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace BookingService.UnitTests;

public class BookingServiceTests
{
    private readonly AppDbContext _context;
    private readonly IBookingService _bookingService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IOutboxRepository _outboxRepository;

    private const int TestEventId = 1;
    private const int TestUserId = 1;

    public BookingServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _bookingRepository = new BookingRepository(_context);
        _outboxRepository = new OutboxRepository(_context);

        var eventPublisherMock = new Mock<IEventPublisher>();
        eventPublisherMock
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var bookingSettings = new BookingSettings
        {
            MaxBookingsPerUser = 10
        };
        var optionsWrapper = Options.Create(bookingSettings);

        _bookingService = new Application.Services.BookingService(
            _bookingRepository,
            optionsWrapper,
            eventPublisherMock.Object,
            _outboxRepository
        );
    }

    private Application.Services.BookingService CreateBookingService(int maxBookingsPerUser)
    {
        var bookingSettings = new BookingSettings
        {
            MaxBookingsPerUser = maxBookingsPerUser
        };
        var optionsWrapper = Options.Create(bookingSettings);

        var eventPublisherMock = new Mock<IEventPublisher>();
        eventPublisherMock
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new Application.Services.BookingService(
            _bookingRepository,
            optionsWrapper,
            eventPublisherMock.Object,
            _outboxRepository
        );
    }

    /// <summary>
    /// Создание брони для существующего события
    /// Проверяем, что возвращается Booking со статусом Pending
    /// Проверяем, что поля заполнены корректно
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_ExistingEvent_ReturnsBookingWithPendingStatus()
    {
        // Arrange
        const int userId = 1;

        // Act
        var result = await _bookingService.CreateBookingAsync(TestEventId, userId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.Equal(TestEventId, result.EventId);
        Assert.Equal(userId, result.UserId);
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
        const int eventId = 1;
        const int userId = 1;

        // Act
        var booking1 = await _bookingService.CreateBookingAsync(eventId, userId, CancellationToken.None);
        var booking2 = await _bookingService.CreateBookingAsync(eventId, userId, CancellationToken.None);
        var booking3 = await _bookingService.CreateBookingAsync(eventId, userId, CancellationToken.None);

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
        const int eventId = 1;
        const int userId = 1;

        var createdBooking = await _bookingService.CreateBookingAsync(eventId, userId, CancellationToken.None);

        // Act
        var result = await _bookingService.GetBookingByIdAsync(createdBooking.Id, CancellationToken.None);

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
        const int eventId = 1;
        const int userId = 1;

        var createdBooking = await _bookingService.CreateBookingAsync(eventId, userId, CancellationToken.None);

        var dbBooking = await _context.Bookings.FindAsync(createdBooking.Id);
        dbBooking!.Confirm();
        await _context.SaveChangesAsync();

        // Act
        var result = await _bookingService.GetBookingByIdAsync(createdBooking.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Confirmed, result.Status);
    }

    /// <summary>
    /// Получение брони по несуществующему Id
    /// Проверяем, что возвращается null
    /// </summary>
    [Fact]
    public async Task GetBookingByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        const int nonExistingBookingId = 999;

        // Act
        var result = await _bookingService.GetBookingByIdAsync(nonExistingBookingId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// После обновления статуса на Confirmed бронь имеет статус Confirmed и заполненный ProcessedAt
    /// </summary>
    [Fact]
    public async Task Booking_WhenConfirmed_SetsStatusConfirmedAndProcessedAt()
    {
        // Arrange
        const int eventId = 1;
        const int userId = 1;

        var booking = await _bookingService.CreateBookingAsync(eventId, userId, CancellationToken.None);

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
        const int eventId = 1;
        const int userId = 1;

        var booking = await _bookingService.CreateBookingAsync(eventId, userId, CancellationToken.None);

        // Act
        booking.Reject();

        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    /// <summary>
    /// Проверка, что повторное подтверждение уже подтвержденной брони выбрасывает исключение
    /// </summary>
    [Fact]
    public void ConfirmAsync_AlreadyConfirmed_ThrowsValidationException()
    {
        // Arrange
        const int eventId = 1;
        const int userId = 1;

        var booking = new Booking(eventId, userId, BookingStatus.Pending);
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
        const int eventId = 1;
        const int userId = 1;

        var booking = new Booking(eventId, userId, BookingStatus.Pending);
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
        const int eventId = 1;
        const int userId = 1;

        var booking = await _bookingService.CreateBookingAsync(eventId, userId, CancellationToken.None);

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
        // Arrange
        const int userId = 1;

        // Act & Assert
        Assert.Throws<ValidationException>(() => new Booking(0, userId, BookingStatus.Pending));
    }

    /// <summary>
    /// Проверка, что бронь нельзя создать, если пользователь достиг лимита
    /// </summary>
    [Fact]
    public async Task Booking_AfterLimit_ThrowsBookingLimitExceededException()
    {
        // Arrange
        const int eventId = 1;
        const int userId = 1;
        const int limit = 3;

        var limitedService = CreateBookingService(limit);

        for (int i = 0; i < limit; i++)
        {
            await limitedService.CreateBookingAsync(eventId, userId, CancellationToken.None);
        }

        // Act & Assert
        await Assert.ThrowsAsync<BookingLimitExceededException>(() =>
            limitedService.CreateBookingAsync(eventId, userId, CancellationToken.None)
        );
    }

    /// <summary>
    /// Проверка, что лимиты бронирований для разных пользователей не влияют друг на друга.
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_WhenTwoUsersHaveSeparateLimits_EachUserHasOwnLimit()
    {
        // Arrange
        const int eventId = 1;
        const int userId1 = 1;
        const int userId2 = 2;
        const int limit = 3;

        var service = CreateBookingService(limit);

        // Act
        for (int i = 0; i < limit; i++)
        {
            await service.CreateBookingAsync(eventId, userId1, CancellationToken.None);
        }

        for (int i = 0; i < limit; i++)
        {
            await service.CreateBookingAsync(eventId, userId2, CancellationToken.None);
        }

        // Assert
        var bookingsUser1 = await _context.Bookings
            .CountAsync(b => b.UserId == userId1);
        Assert.Equal(limit, bookingsUser1);

        var bookingsUser2 = await _context.Bookings
            .CountAsync(b => b.UserId == userId2);
        Assert.Equal(limit, bookingsUser2);
    }

    /// <summary>
    /// Проверка, что лимиты работают только для статусов Pending/Confirmed (только активные брони)
    /// </summary>
    [Fact]
    public async Task CountBookingsByUserIdAsync_WhenBookingCancelled_DoesNotCount()
    {
        // Arrange
        const int eventId = 1;
        const int userId = 1;

        var booking = await _bookingService.CreateBookingAsync(eventId, userId, CancellationToken.None);

        booking.Cancel();
        await _context.SaveChangesAsync();

        // Act
        var count = await _bookingRepository.CountBookingsByUserIdAsync(userId);

        // Assert
        Assert.Equal(0, count);
    }
}