using EventOrchestrationService.Data;
using EventOrchestrationService.Exceptions;
using EventOrchestrationService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.Tests;

public class BookingServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IBookingService _service;

    public BookingServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _service = new BookingService(_context);
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
                EndAt = DateTime.UtcNow.AddDays(5), TotalSeats = 10
            },
            new Event
            {
                Id = 2, Title = "Title2", Description = "Description2", StartAt = DateTime.UtcNow.AddDays(-5),
                EndAt = DateTime.UtcNow.AddDays(5), TotalSeats = 10
            }
        );
        _context.SaveChanges();
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

        var eventService = new EventService(_context, new Event.EventValidator());
        eventService.DeleteEvent(2);

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
}