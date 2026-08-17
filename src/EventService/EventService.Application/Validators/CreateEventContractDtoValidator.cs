using EventOrchestrationService.Contracts.DTOs;
using EventService.Domain.Constants;
using FluentValidation;

namespace EventService.Application.Validators;

public class CreateEventContractDtoValidator : AbstractValidator<CreateEventContractDto>
{
    public CreateEventContractDtoValidator()
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