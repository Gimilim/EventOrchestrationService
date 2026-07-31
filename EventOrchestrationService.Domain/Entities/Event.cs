using EventOrchestrationService.Domain.Constants;
using EventOrchestrationService.Domain.Enums;
using EventOrchestrationService.Domain.Exceptions;

namespace EventOrchestrationService.Domain.Entities;

public class Event
{
    private readonly object _seatsLock = new();

    private Event()
    {
    }

    public Event(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("Название события обязательно для заполнения");

        if (endAt <= startAt)
            throw new ValidationException("Дата окончания должна быть больше даты начала");

        if (totalSeats <= 0)
            throw new ValidationException("Общее количество мест должно быть больше нуля");

        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }

    public int Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public int TotalSeats { get; private set; }
    public int AvailableSeats { get; private set; }
    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
    public uint RowVersion { get; private set; }

    public ReservationResult TryReserveSeats(int count = 1)
    {
        lock (_seatsLock)
        {
            if (DateTime.UtcNow >= StartAt)
                return ReservationResult.EventAlreadyStarted;

            if (AvailableSeats < count)
                return ReservationResult.NoAvailableSeats;

            AvailableSeats -= count;
            return ReservationResult.Success;
        }
    }

    public Event(int id, string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
        : this(title, description, startAt, endAt, totalSeats)
    {
        Id = id;
    }

    public bool ReleaseSeats(int count = 1)
    {
        lock (_seatsLock)
        {
            if (count <= 0) return false;

            var newAvailable = AvailableSeats + count;
            if (newAvailable > TotalSeats)
                newAvailable = TotalSeats;

            AvailableSeats = newAvailable;
            return true;
        }
    }

    public void Update(string? title, string? description, DateTime? startAt, DateTime? endAt, int? totalSeats)
    {
        if (title != null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ValidationException("Название события обязательно для заполнения");

            Title = title;
        }

        if (description != null)
        {
            if (description.Length > EventConstants.MaxDescriptionLength)
                throw new ValidationException(
                    $"Описание не может быть длиннее {EventConstants.MaxDescriptionLength} символов");

            Description = description;
        }

        var newStartAt = startAt ?? StartAt;
        var newEndAt = endAt ?? EndAt;

        if (newEndAt <= newStartAt)
            throw new ValidationException("Дата окончания должна быть больше даты начала");

        if (startAt.HasValue)
        {
            StartAt = startAt.Value;
        }

        if (endAt.HasValue)
        {
            EndAt = endAt.Value;
        }

        if (totalSeats.HasValue)
        {
            if (totalSeats <= 0)
                throw new ValidationException("Общее количество мест должно быть больше нуля");

            var reservedSeats = TotalSeats - AvailableSeats;

            if (totalSeats < reservedSeats)
                throw new ValidationException(
                    $"Нельзя уменьшить количество мест до {totalSeats}, так как уже занято {reservedSeats} мест");

            TotalSeats = totalSeats.Value;
            AvailableSeats = totalSeats.Value - reservedSeats;
        }
    }
}