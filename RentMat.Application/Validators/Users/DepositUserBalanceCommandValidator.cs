using FluentValidation;
using RentMat.Application.Commands.Users;
using RentMat.Core.Constants;

namespace RentMat.Application.Validators.Users;

public class DepositUserBalanceCommandValidator : AbstractValidator<DepositUserBalanceCommand>
{
    public DepositUserBalanceCommandValidator()
    {
        RuleFor(d => d.Amount)
            .GreaterThan(0)
            .LessThanOrEqualTo(ValidationConstants.MaxDepositAmount);
        RuleFor(d => d.UserId)
            .GreaterThan(0);
    }
}