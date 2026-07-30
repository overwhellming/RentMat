using FluentValidation;
using RentMat.Application.Commands.Authentication;

namespace RentMat.Application.Validators.Authentication;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .Cascade(CascadeMode.Stop)
            .NotEmpty();

        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}