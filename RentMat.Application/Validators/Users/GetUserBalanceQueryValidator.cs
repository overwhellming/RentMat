using FluentValidation;
using RentMat.Application.Queries.Users;

namespace RentMat.Application.Validators.Users;

public class GetUserBalanceQueryValidator : AbstractValidator<GetUserBalanceQuery>
{
    public GetUserBalanceQueryValidator()
    {
        RuleFor(d => d.UserId)
            .GreaterThan(0);
    }
}