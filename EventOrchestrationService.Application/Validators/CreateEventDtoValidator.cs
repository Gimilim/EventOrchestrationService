using FluentValidation;
using EventOrchestrationService.Application.DTOs;
using EventOrchestrationService.Domain.Constants;

namespace EventOrchestrationService.Application.Validators;

public class CreateEventDtoValidator : AbstractValidator<CreateEventDto>
{
    public CreateEventDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Название события обязательно")
            .MaximumLength(EventConstants.MaxTitleLength)
            .WithMessage($"Название не может быть длиннее {EventConstants.MaxTitleLength} символов");

        RuleFor(x => x.Description)
            .MaximumLength(EventConstants.MaxDescriptionLength)
            .WithMessage($"Описание не может быть длиннее {EventConstants.MaxDescriptionLength} символов");

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt)
            .WithMessage("Дата окончания должна быть больше даты начала")
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Дата окончания не может быть в прошлом");

        RuleFor(x => x.TotalSeats)
            .GreaterThan(0)
            .WithMessage("Общее количество мест должно быть больше нуля");
    }
}