using EventOrchestrationService.Contracts.Events;
using EventService.Application.Interfaces;
using EventService.Application.Services;
using EventService.Domain.Entities;
using EventService.Domain.Enums;
using EventService.Infrastructure.Data.Repositories;
using EventService.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventService.IntegrationTests;

[Collection("Database collection")]
public class BookingValidationIntegrationTests : IntegrationTestBase
{
    private readonly IBookingValidationService _bookingValidationService;
    private readonly IEventRepository _eventRepository;
    private readonly IInboxRepository _inboxRepository;
    private readonly Mock<IEventPublisher> _eventPublisherMock;

    public BookingValidationIntegrationTests(PostgreSqlContainerFixture fixture) : base(fixture)
    {
        var context = CreateContext();

        _eventRepository = new EventRepository(context);
        _inboxRepository = new InboxRepository(context);

        var cacheMock = new Mock<ICacheService>();
        cacheMock
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _eventPublisherMock = new Mock<IEventPublisher>();
        _eventPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _bookingValidationService = new BookingValidationService(
            _eventRepository,
            _eventPublisherMock.Object,
            Mock.Of<ILogger<BookingValidationService>>(),
            _inboxRepository,
            cacheMock.Object
        );
    }

    /// <summary>
    /// Проверка, что при несуществующем событии публикуется BookingRejectedEvent с причиной "Event not found"
    /// </summary>
    [Fact]
    public async Task ValidateBookingAsync_EventNotFound_PublishesRejectedEvent()
    {
        // Arrange
        const int nonExistingEventId = 999;
        const int bookingId = 1;

        var evt = new BookingCreatedEvent
        {
            BookingId = bookingId,
            EventId = nonExistingEventId,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _bookingValidationService.ValidateBookingAsync(evt, CancellationToken.None);

        // Assert
        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                "booking-rejected",
                It.Is<BookingRejectedEvent>(e => e.BookingId == bookingId && e.Reason == "Событие с ID 999 не найдено"),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    /// <summary>
    /// Проверка, что при удалённом событии публикуется BookingRejectedEvent с причиной "Event not found"
    /// </summary>
    [Fact]
    public async Task ValidateBookingAsync_EventDeleted_PublishesRejectedEvent()
    {
        // Arrange
        const int eventId = 1;
        const int bookingId = 1;

        var testEvent = new Event(
            title: "Test Event",
            description: "Test Description",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10
        );

        await _eventRepository.AddAsync(testEvent);
        await _eventRepository.SaveChangesAsync();

        await _eventRepository.DeleteAsync(testEvent);
        await _eventRepository.SaveChangesAsync();

        var evt = new BookingCreatedEvent
        {
            BookingId = bookingId,
            EventId = eventId,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        await _bookingValidationService.ValidateBookingAsync(evt, CancellationToken.None);

        // Assert
        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                "booking-rejected",
                It.Is<BookingRejectedEvent>(e =>
                    e.BookingId == bookingId && e.Reason == $"Событие с ID {eventId} не найдено"),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    /// <summary>
    /// Проверка, что успешное резервирование уменьшает AvailableSeats на 1
    /// </summary>
    [Fact]
    public async Task ValidateBookingAsync_ValidBooking_DecreasesAvailableSeatsByOne()
    {
        // Arrange
        const int eventId = 1;
        const int bookingId = 1;

        var testEvent = new Event(
            title: "Test Event",
            description: "Test Description",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10
        );

        await _eventRepository.AddAsync(testEvent);
        await _eventRepository.SaveChangesAsync();

        var eventBefore = await _eventRepository.GetByIdAsync(eventId);
        var seatsBefore = eventBefore!.AvailableSeats;

        var evt = new BookingCreatedEvent
        {
            BookingId = bookingId,
            EventId = eventId,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _bookingValidationService.ValidateBookingAsync(evt, CancellationToken.None);

        // Assert
        var eventAfter = await _eventRepository.GetByIdAsync(eventId);
        Assert.Equal(seatsBefore - 1, eventAfter!.AvailableSeats);
    }

    /// <summary>
    /// Проверка, что при отсутствии свободных мест публикуется BookingRejectedEvent с причиной "No available seats"
    /// </summary>
    [Fact]
    public async Task ValidateBookingAsync_NoSeatsLeft_PublishesRejectedEvent()
    {
        // Arrange
        const int eventId = 1;
        const int totalSeats = 5;

        var testEvent = new Event(
            title: "Test Event",
            description: "Test Description",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: totalSeats
        );

        await _eventRepository.AddAsync(testEvent);
        await _eventRepository.SaveChangesAsync();

        for (int i = 1; i <= totalSeats; i++)
        {
            var evt = new BookingCreatedEvent
            {
                BookingId = i,
                EventId = eventId,
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            };
            await _bookingValidationService.ValidateBookingAsync(evt, CancellationToken.None);
        }

        var overbookEvt = new BookingCreatedEvent
        {
            BookingId = totalSeats + 1,
            EventId = eventId,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _bookingValidationService.ValidateBookingAsync(overbookEvt, CancellationToken.None);

        // Assert
        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                "booking-rejected",
                It.Is<BookingRejectedEvent>(e =>
                    e.BookingId == totalSeats + 1 &&
                    e.Reason == $"На событие с ID {eventId} нет свободных мест"),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    /// <summary>
    /// Проверка, что после отмены брони (ReleaseSeats) количество свободных мест восстанавливается
    /// </summary>
    [Fact]
    public async Task HandleBookingCancelledAsync_ReleaseSeats_RestoresAvailableSeats()
    {
        // Arrange
        const int eventId = 1;
        const int bookingId = 1;

        var testEvent = new Event(
            title: "Test Event",
            description: "Test Description",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10
        );

        await _eventRepository.AddAsync(testEvent);
        await _eventRepository.SaveChangesAsync();

        var eventBefore = await _eventRepository.GetByIdAsync(eventId);
        var seatsBefore = eventBefore!.AvailableSeats;

        var createEvt = new BookingCreatedEvent
        {
            BookingId = bookingId,
            EventId = eventId,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };
        await _bookingValidationService.ValidateBookingAsync(createEvt, CancellationToken.None);

        var eventAfterBooking = await _eventRepository.GetByIdAsync(eventId);
        Assert.Equal(seatsBefore - 1, eventAfterBooking!.AvailableSeats);

        var cancelEvt = new BookingCancelledEvent
        {
            BookingId = bookingId,
            EventId = eventId,
            UserId = 1
        };
        await _bookingValidationService.HandleBookingCancelledAsync(cancelEvt, CancellationToken.None);

        // Assert
        var eventAfterRelease = await _eventRepository.GetByIdAsync(eventId);
        Assert.Equal(seatsBefore, eventAfterRelease!.AvailableSeats);
    }

    /// <summary>
    /// Проверка, что после отмены брони и возврата мест можно создать новую бронь
    /// </summary>
    [Fact]
    public async Task HandleBookingCancelledAsync_AfterReject_AllowsNewBookingOnSameSeat()
    {
        // Arrange
        const int eventId = 1;
        const int bookingId1 = 1;
        const int bookingId2 = 2;

        var testEvent = new Event(
            title: "Test Event",
            description: "Test Description",
            startAt: DateTime.UtcNow.AddDays(1),
            endAt: DateTime.UtcNow.AddDays(2),
            totalSeats: 10
        );

        await _eventRepository.AddAsync(testEvent);
        await _eventRepository.SaveChangesAsync();

        var eventBefore = await _eventRepository.GetByIdAsync(eventId);
        var seatsBefore = eventBefore!.AvailableSeats;

        var createEvt1 = new BookingCreatedEvent
        {
            BookingId = bookingId1,
            EventId = eventId,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };
        await _bookingValidationService.ValidateBookingAsync(createEvt1, CancellationToken.None);

        var cancelEvt = new BookingCancelledEvent
        {
            BookingId = bookingId1,
            EventId = eventId,
            UserId = 1
        };
        await _bookingValidationService.HandleBookingCancelledAsync(cancelEvt, CancellationToken.None);

        var createEvt2 = new BookingCreatedEvent
        {
            BookingId = bookingId2,
            EventId = eventId,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };
        await _bookingValidationService.ValidateBookingAsync(createEvt2, CancellationToken.None);

        // Assert
        var eventAfter = await _eventRepository.GetByIdAsync(eventId);
        Assert.Equal(seatsBefore - 1, eventAfter!.AvailableSeats);
    }

    /// <summary>
    /// Проверка, что при попытке создать бронь на событие, которое уже началось,
    /// публикуется BookingRejectedEvent с причиной "Event already started"
    /// </summary>
    [Fact]
    public async Task ValidateBookingAsync_EventAlreadyStarted_PublishesRejectedEvent()
    {
        // Arrange
        const int eventId = 1;
        const int bookingId = 1;

        var testEvent = new Event(
            title: "Test Event",
            description: "Test Description",
            startAt: DateTime.UtcNow.AddDays(-1), // Вчера
            endAt: DateTime.UtcNow.AddDays(1),
            totalSeats: 10
        );

        await _eventRepository.AddAsync(testEvent);
        await _eventRepository.SaveChangesAsync();

        var evt = new BookingCreatedEvent
        {
            BookingId = bookingId,
            EventId = eventId,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _bookingValidationService.ValidateBookingAsync(evt, CancellationToken.None);

        // Assert
        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                "booking-rejected",
                It.Is<BookingRejectedEvent>(e =>
                    e.BookingId == bookingId &&
                    e.Reason == $"Событие с ID {eventId} уже началось"),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}