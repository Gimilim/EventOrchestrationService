using EventOrchestrationService.Contracts.DTOs;
using EventService.Domain.Constants;
using FluentValidation;

namespace EventService.Application.Validators;

public class UpdateEventContractDtoValidator : AbstractValidator<UpdateEventContractDto>
{
    public UpdateEventContractDtoValidator()
    {
        RuleFor(x => x.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Название события не может быть пустым")
            .MaximumLength(EventConstants.MaxTitleLength)
            .WithMessage($"Название не может быть длиннее {EventConstants.MaxTitleLength} символов")
            .When(x => x.Title != null);

        RuleFor(x => x.Description)
            .MaximumLength(EventConstants.MaxDescriptionLength)
            .WithMessage($"Описание не может быть длиннее {EventConstants.MaxDescriptionLength} символов")
            .When(x => x.Description != null);

        RuleFor(x => x.EndAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Дата окончания не может быть в прошлом")
            .GreaterThan(x => x.StartAt)
            .WithMessage("Дата окончания должна быть больше даты начала")
            .When(x => x.EndAt.HasValue);

        RuleFor(x => x.TotalSeats)
            .GreaterThan(0)
            .WithMessage("Общее количество мест должно быть больше нуля")
            .When(x => x.TotalSeats.HasValue);
    }
}