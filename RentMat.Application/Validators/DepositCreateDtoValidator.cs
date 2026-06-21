using FluentValidation;
using RentMat.Application.DTOs.User;

namespace RentMat.Application.Validators;

public class DepositCreateDtoValidator : AbstractValidator<DepositCreateDto>
{
    public DepositCreateDtoValidator()
    {
        RuleFor(d => d.Amount)
            .GreaterThanOrEqualTo(0);
    }
}