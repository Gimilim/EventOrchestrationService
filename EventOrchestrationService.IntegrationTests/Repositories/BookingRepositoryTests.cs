using EventOrchestrationService.Data.Repositories.Implementations;
using EventOrchestrationService.Entities;
using EventOrchestrationService.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.IntegrationTests.Repositories;

public class BookingRepositoryTests(PostgreSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    private async Task<Event> SeedTestEventAsync()
    {
        var testEvent = new Event
        {
            Title = "Title1", Description = "Description1", StartAt = DateTime.UtcNow.AddDays(-5),
            EndAt = DateTime.UtcNow.AddDays(3), TotalSeats = 10, AvailableSeats = 10
        };

        await using var context = CreateContext();
        await context.Events.AddAsync(testEvent);
        await context.SaveChangesAsync();
        return testEvent;
    }

    [Fact]
    public async Task AddAsync_ValidBooking_AddsBookingToDatabase()
    {
        // Arrange
        var testEvent = await SeedTestEventAsync();

        var newBooking = new Booking
        {
            EventId = testEvent.Id,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = null
        };

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
        Assert.Equal(newBooking.CreatedAt, savedBooking.CreatedAt, TimeSpan.FromSeconds(1));
        Assert.Null(savedBooking.ProcessedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsBookingWithCorrectData()
    {
        // Arrange
        var testEvent = await SeedTestEventAsync();

        var booking = new Booking
        {
            EventId = testEvent.Id,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow.AddMinutes(5)
        };

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
}