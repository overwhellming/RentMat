using FluentValidation;
using RentMat.Application.DTOs.User;
using RentMat.Core.Constants;

namespace RentMat.Application.Validators.Users;

public class DepositCreateDtoValidator : AbstractValidator<DepositCreateDto>
{
    public DepositCreateDtoValidator()
    {
        RuleFor(d => d.Amount)
            .GreaterThan(0)
            .LessThanOrEqualTo(ValidationConstants.MaxDepositAmount);
    }
}