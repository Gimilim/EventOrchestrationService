using EventOrchestrationService.Domain.Entities;
using EventOrchestrationService.Domain.Enums;
using EventOrchestrationService.Domain.Exceptions;
using EventOrchestrationService.Infrastructure.Data.Repositories;
using EventOrchestrationService.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.IntegrationTests.Repositories;

public class BookingRepositoryTests(PostgreSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    private async Task<Event> SeedTestEventAsync()
    {
        var testEvent = new Event(
            title: "Title1",
            description: "Description1",
            startAt: DateTime.UtcNow.AddDays(-5),
            endAt: DateTime.UtcNow.AddDays(3),
            totalSeats: 10
        );

        await using var context = CreateContext();
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();
        return testEvent;
    }

    private async Task<User> SeedTestUserAsync()
    {
        var testUser = new User(
            "testLogin",
            "testPassword",
            Role.Admin);

        await using var context = CreateContext();
        await context.Users.AddAsync(testUser);
        await context.SaveChangesAsync();
        return testUser;
    }

    [Fact]
    public async Task AddAsync_ValidBooking_AddsBookingToDatabase()
    {
        // Arrange
        var testEvent = await SeedTestEventAsync();
        var testUser = await SeedTestUserAsync();

        var newBooking = new Booking(testEvent.Id, testUser.Id, BookingStatus.Pending);

        // Act
        await using (var context = CreateContext())
        {
            var repository = new BookingRepository(context);
            await repository.AddAsync(newBooking);
            await repository.SaveChangesAsync();
        }

        // Assert
        await using var verifyContext = CreateContext();
        var savedBooking = await verifyContext.Bookings
            .FirstOrDefaultAsync(b => b.EventId == testEvent.Id);

        Assert.NotNull(savedBooking);
        Assert.True(savedBooking.Id > 0);
        Assert.Equal(BookingStatus.Pending, savedBooking.Status);
        Assert.Equal(DateTime.UtcNow, savedBooking.CreatedAt, TimeSpan.FromSeconds(5));
        Assert.Null(savedBooking.ProcessedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsBookingWithCorrectData()
    {
        // Arrange
        var testEvent = await SeedTestEventAsync();
        var testUser = await SeedTestUserAsync();

        var booking = new Booking(testEvent.Id, testUser.Id, BookingStatus.Pending);
        booking.Confirm();

        await using (var context = CreateContext())
        {
            var repository = new BookingRepository(context);
            await repository.AddAsync(booking);
            await repository.SaveChangesAsync();
        }

        // Act
        await using var actContext = CreateContext();
        var actRepository = new BookingRepository(actContext);
        var result = await actRepository.GetByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(BookingStatus.Confirmed, result.Status);
        Assert.NotNull(result.ProcessedAt);
        Assert.Equal(booking.ProcessedAt.Value, result.ProcessedAt.Value, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        const int nonExistingBookingId = -1;

        // Act
        await using var actContext = CreateContext();
        var actRepository = new BookingRepository(actContext);
        var result = await actRepository.GetByIdAsync(nonExistingBookingId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_InvalidEventId_ThrowsValidationException()
    {
        // Act & Assert
        var testUser = await SeedTestUserAsync();
        Assert.Throws<ValidationException>(() => new Booking(0, testUser.Id, BookingStatus.Pending));
    }

    [Fact]
    public async Task ConfirmAsync_AlreadyConfirmed_ThrowsValidationException()
    {
        // Arrange
        var testUser = await SeedTestUserAsync();
        var booking = new Booking(1, testUser.Id, BookingStatus.Pending);
        booking.Confirm();

        // Act & Assert
        Assert.Throws<ValidationException>(() => booking.Confirm());
    }

    [Fact]
    public async Task ConfirmAsync_ValidBooking_UpdatesStatusAndProcessedAt()
    {
        // Arrange
        var testEvent = await SeedTestEventAsync();
        var testUser = await SeedTestUserAsync();

        var booking = new Booking(testEvent.Id, testUser.Id, BookingStatus.Pending);

        await using (var context = CreateContext())
        {
            var repository = new BookingRepository(context);
            await repository.AddAsync(booking);
            await repository.SaveChangesAsync();

            // Act
            booking.Confirm();
            await repository.SaveChangesAsync();

            // Assert
            await using var verifyContext = CreateContext();
            var savedBooking = await verifyContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == booking.Id);

            Assert.NotNull(savedBooking);
            Assert.Equal(booking.Id, savedBooking.Id);
            Assert.Equal(BookingStatus.Confirmed, savedBooking.Status);
            Assert.NotNull(savedBooking.ProcessedAt);
            Assert.Equal(booking.ProcessedAt!.Value, savedBooking.ProcessedAt!.Value, TimeSpan.FromSeconds(1));
        }
    }
}