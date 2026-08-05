using FluentValidation;
using RentMat.Application.Commands.Authentication;

namespace RentMat.Application.Validators.Authentication;

public class RevokeRefreshTokenCommandValidator : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RevokeRefreshTokenCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);
    }
}