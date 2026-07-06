using System.ComponentModel.DataAnnotations;

namespace EventOrchestrationService.Entities;

public class Booking
{
    [Required(ErrorMessage = "ИД бронирования обязателен для заполнения")]
    public int Id { get; set; }

    [Required(ErrorMessage = "ИД события, к которому относится бронирование, обязателен для заполнения")]
    public int EventId { get; set; }

    [Required(ErrorMessage = "Текущий статус бронирования обязателен для заполнения")]
    public BookingStatus Status { get; set; }

    [Required(ErrorMessage = "Дата и время создания бронирования обязательны для заполнения")]
    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public Event Event { get; set; }
}

public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    Rejected = 3
}