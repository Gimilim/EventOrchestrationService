using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
using BookingService.Infrastructure.Data.Repositories;
using BookingService.IntegrationTests.Fixtures;
using DomainValidationException = EventOrchestrationService.Contracts.Exceptions.ValidationException;
using Microsoft.EntityFrameworkCore;

namespace BookingService.IntegrationTests.Repositories;

public class BookingRepositoryTests(PostgreSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    private const int TestEventId = 1;
    private const int TestUserId = 1;

    [Fact]
    public async Task AddAsync_ValidBooking_AddsBookingToDatabase()
    {
        // Arrange
        var newBooking = new Booking(TestEventId, TestUserId, BookingStatus.Pending);

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
            .FirstOrDefaultAsync(b => b.EventId == TestEventId);

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
        var booking = new Booking(TestEventId, TestUserId, BookingStatus.Pending);
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
    public void AddAsync_InvalidEventId_ThrowsValidationException()
    {
        // Act & Assert
        Assert.Throws<DomainValidationException>(() => new Booking(0, TestUserId, BookingStatus.Pending));
    }

    [Fact]
    public void ConfirmAsync_AlreadyConfirmed_ThrowsValidationException()
    {
        // Arrange
        var booking = new Booking(1, TestUserId, BookingStatus.Pending);
        booking.Confirm();

        // Act & Assert
        Assert.Throws<DomainValidationException>(() => booking.Confirm());
    }

    [Fact]
    public async Task ConfirmAsync_ValidBooking_UpdatesStatusAndProcessedAt()
    {
        // Arrange
        var booking = new Booking(TestEventId, TestUserId, BookingStatus.Pending);

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