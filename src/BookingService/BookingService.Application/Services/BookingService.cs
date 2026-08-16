using BookingService.Application.Interfaces;
using BookingService.Application.Settings;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
using BookingService.Domain.Exceptions;
using EventOrchestrationService.Contracts.Events;
using EventOrchestrationService.Contracts.Exceptions;
using Microsoft.Extensions.Options;

namespace BookingService.Application.Services;

public class BookingService(
    IBookingRepository bookingRepository,
    IOptions<BookingSettings> settings,
    IEventPublisher eventPublisher) : IBookingService
{
    public async Task<Booking> CreateBookingAsync(int eventId, int userId, CancellationToken cancellationToken)
    {
        await EnsureUserCanBookAsync(userId, cancellationToken);

        var createdBooking = new Booking(eventId, userId, BookingStatus.Pending);

        await bookingRepository.AddAsync(createdBooking, cancellationToken);
        await bookingRepository.SaveChangesAsync(cancellationToken);

        var evt = new BookingCreatedEvent
        {
            BookingId = createdBooking.Id,
            EventId = eventId,
            UserId = userId,
            CreatedAt = createdBooking.CreatedAt
        };

        await eventPublisher.PublishAsync("booking-created", evt, key: eventId.ToString(), cancellationToken);

        return createdBooking;
    }

    private async Task EnsureUserCanBookAsync(int userId, CancellationToken cancellationToken)
    {
        var currentBookings = await bookingRepository.CountBookingsByUserIdAsync(userId, cancellationToken);
        var maxBookingsPerUser = settings.Value.MaxBookingsPerUser;
        if (currentBookings >= maxBookingsPerUser)
            throw new BookingLimitExceededException(
                $"Пользователь с ID {userId} превысил лимит бронирований ({maxBookingsPerUser})");
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken)
    {
        return await bookingRepository.GetByIdAsync(bookingId, cancellationToken);
    }

    public async Task CancelBookingAsync(int bookingId, int userId, bool skipCancelPermission,
        CancellationToken cancellationToken)
    {
        var targetBooking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken);

        if (targetBooking is null)
            throw new NotFoundException($"Бронирование с ID {bookingId} не найдено");

        if (!skipCancelPermission && targetBooking.UserId != userId)
            throw new AccessDeniedException("Можно отменить только свое бронирование.");

        targetBooking.Cancel();

        await bookingRepository.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            "booking-cancelled",
            new BookingCancelledEvent
            {
                BookingId = targetBooking.Id,
                EventId = targetBooking.EventId,
                UserId = targetBooking.UserId
            },
            key: targetBooking.EventId.ToString(),
            cancellationToken: cancellationToken
        );
    }
}