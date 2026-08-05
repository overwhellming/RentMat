using FluentValidation;
using RentMat.Application.Queries.Users;

namespace RentMat.Application.Validators.Users;

public class GetUserDepositsQueryValidator : AbstractValidator<GetUserDepositsQuery>
{
    public GetUserDepositsQueryValidator()
    {
        RuleFor(d => d.UserId)
            .GreaterThan(0);
    }
}