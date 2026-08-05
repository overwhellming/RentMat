using FluentValidation.TestHelper;
using RentMat.Application.Commands.Booking;
using RentMat.Application.Queries.Booking;
using RentMat.Application.Validators.Booking;

namespace RentMat.Application.UnitTests.Validators.Booking;

public class GetUserBookingsQueryValidatorTests
{
    private readonly GetUserBookingsQueryValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Query_Is_Correct()
    {
        var query = new GetUserBookingsQuery
        ( 
            UserId: 1
        );

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_UserId_Is_Zero()
    {
        var query = new GetUserBookingsQuery
        ( 
            UserId: 0
        );

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    } 
}