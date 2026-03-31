using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace EventOrchestrationService.Models;

public class Event
{
    [Required(ErrorMessage = "ИД события обязателен для заполнения")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Название события обязательно для заполнения")]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required(ErrorMessage = "Дата начала события обязательно для заполнения")]
    public DateTime StartAt { get; set; }

    [Required(ErrorMessage = "Дата окончания события обязательно для заполнения")]
    public DateTime EndAt { get; set; }

    public class EventValidator : AbstractValidator<Event>
    {
        public EventValidator()
        {
            RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt)
                .WithMessage("Дата окончания должна быть больше даты начала");
        }
    }

}