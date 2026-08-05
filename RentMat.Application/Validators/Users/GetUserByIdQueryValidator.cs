using FluentValidation;
using RentMat.Application.Queries.Users;

namespace RentMat.Application.Validators.Users;

public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(d => d.UserId)
            .GreaterThan(0);
    }
}