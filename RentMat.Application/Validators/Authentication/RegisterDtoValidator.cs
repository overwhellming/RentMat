using FluentValidation;
using RentMat.Application.DTOs.Authentication;
using RentMat.Core.Constants;

namespace RentMat.Application.Validators.Authentication;

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
            .MaximumLength(ValidationConstants.UserLoginMaxLength);

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(ValidationConstants.UserPasswordMinLength)
            .MaximumLength(ValidationConstants.UserPasswordMaxLength);
    }
}