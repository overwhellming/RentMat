using FluentValidation;
using RentMat.Application.DTOs.Authentication;

namespace RentMat.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Login)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Matches(@"^\S+$")
            .MaximumLength(50);

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(50);
    }
}