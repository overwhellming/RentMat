using FluentValidation.TestHelper;
using RentMat.Application.Queries.Users;
using RentMat.Application.Validators.Users;

namespace RentMat.Application.UnitTests.Validators.Users;

public class GetUserByIdQueryValidatorTests
{
    private readonly GetUserByIdQueryValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Query_Is_Correct()
    {
        var query = new GetUserByIdQuery
        (
            UserId: 1
        );

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_UserId_Is_Zero()
    {
        var query = new GetUserByIdQuery
        (
            UserId: 0
        );

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}