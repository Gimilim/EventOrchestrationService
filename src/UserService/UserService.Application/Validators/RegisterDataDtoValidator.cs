using FluentValidation;
using UserService.Application.DTOs;
using UserService.Domain.Constants;

namespace UserService.Application.Validators;

public class RegisterDataDtoValidator : AbstractValidator<RegisterDataDto>
{
    public RegisterDataDtoValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty()
            .WithMessage("Логин обязателен к заполнению.")
            .MaximumLength(UserConstants.MaxLoginLength)
            .WithMessage($"Логин не может быть длиннее {UserConstants.MaxLoginLength} символов")
            .MinimumLength(UserConstants.MinLoginLength)
            .WithMessage($"Логин не может быть короче {UserConstants.MinLoginLength} символов");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Пароль обязателен к заполнению.")
            .MaximumLength(UserConstants.MaxPasswordLength)
            .WithMessage($"Пароль не может быть длиннее {UserConstants.MaxPasswordLength} символов")
            .MinimumLength(UserConstants.MinPasswordLength)
            .WithMessage($"Пароль не может быть короче {UserConstants.MinPasswordLength} символов");
    }
}