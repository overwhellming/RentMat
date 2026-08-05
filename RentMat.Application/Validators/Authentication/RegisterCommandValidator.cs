using FluentValidation;
using RentMat.Application.Commands.Authentication;
using RentMat.Core.Constants;

namespace RentMat.Application.Validators.Authentication;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
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
            .NotEmpty()
            .MinimumLength(ValidationConstants.UserPasswordMinLength)
            .MaximumLength(ValidationConstants.UserPasswordMaxLength);
    }
}