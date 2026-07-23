using EventOrchestrationService.Domain.Enums;
using EventOrchestrationService.Domain.Exceptions;

namespace EventOrchestrationService.Domain.Entities;

public class Booking
{
    private Booking()
    {
    }

    public Booking(int eventId, int userId, BookingStatus status)
    {
        if (eventId <= 0)
            throw new ValidationException("ИД события обязательно для заполнения");

        if (userId <= 0)
            throw new ValidationException("ИД пользователя обязательно для заполнения");

        EventId = eventId;
        UserId = userId;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public int EventId { get; private set; }
    public int UserId { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public Event Event { get; private set; }
    public User User { get; private set; }

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new ValidationException("Только брони в статусе 'В обработке' могут быть подтверждены");

        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status == BookingStatus.Rejected)
            throw new ValidationException("Бронь уже отклонена");

        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }
}