using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace EventOrchestrationService.Models;

public class Event
{
    private readonly object _seatsLock = new();

    [Required(ErrorMessage = "ИД события обязателен для заполнения")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Название события обязательно для заполнения")]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Дата начала события обязательно для заполнения")]
    public required DateTime StartAt { get; set; }

    [Required(ErrorMessage = "Дата окончания события обязательно для заполнения")]
    public required DateTime EndAt { get; set; }

    [Required(ErrorMessage = "Общее количество мест на событие обязательно для заполнения")]
    public required int TotalSeats { get; set; }

    public int AvailableSeats { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

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

    public class EventValidator : AbstractValidator<Event>
    {
        public EventValidator()
        {
            RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt)
                .WithMessage("Дата окончания должна быть больше даты начала");

            RuleFor(x => x.TotalSeats)
                .GreaterThan(0)
                .WithMessage("Общее количество мест должно быть больше нуля");
        }
    }
}