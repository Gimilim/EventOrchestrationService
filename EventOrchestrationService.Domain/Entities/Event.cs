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

    public bool TryReserveSeats(int count = 1)
    {
        lock (_seatsLock)
        {
            if (AvailableSeats >= count)
            {
                AvailableSeats -= count;
                return true;
            }

            return false;
        }
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
}