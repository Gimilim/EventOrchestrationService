using FluentValidation;
using UserService.Application.DTOs;
using UserService.Domain.Constants;

namespace UserService.Application.Validators;

public class LoginDataDtoValidator : AbstractValidator<LoginDataDto>
{
    public LoginDataDtoValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Логин обязателен для заполнения.")
            .MaximumLength(UserConstants.MaxLoginLength)
            .WithMessage($"Логин не может быть длиннее {UserConstants.MaxLoginLength} символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен для заполнения.")
            .MaximumLength(UserConstants.MaxPasswordLength)
            .WithMessage($"Пароль не может быть длиннее {UserConstants.MaxPasswordLength} символов");
    }
}