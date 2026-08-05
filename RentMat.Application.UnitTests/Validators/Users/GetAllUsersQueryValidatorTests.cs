using FluentValidation.TestHelper;
using RentMat.Application.Handlers.Booking;
using RentMat.Application.Queries.Booking;
using RentMat.Application.Queries.Users;
using RentMat.Application.Validators.Devices;
using RentMat.Core.Constants;

namespace RentMat.Application.UnitTests.Validators.Devices;

public class GetAllUsersQueryValidatorTests
{
    private readonly GetAllUsersQueryValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Query_Is_Correct()
    {
        var query = new GetAllUsersQuery
        (
            Page: 1,
            PageSize: 10
        );

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Should_HaveError_When_Page_Is_Zero()
    {
        var query = new GetAllUsersQuery
        (
            Page: 0,
            PageSize: 10
        );

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }
    
    [Fact]
    public void Should_HaveError_When_PageSize_Is_Zero()
    {
        var query = new GetAllUsersQuery
        (
            Page: 1,
            PageSize: 0
        );

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
    
    [Fact]
    public void Should_HaveError_When_PageSize_Exceed_MaxPageSize()
    {
        var query = new GetAllUsersQuery
        (
            Page: 1,
            PageSize: GetAllBookingsQueryHandler.MaxPageSize + 1
        );

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
    
    [Fact]
    public void Should_HaveError_When_Search_Exceed_MaxLength()
    {
        var query = new GetAllUsersQuery
        (
            Page: 1,
            PageSize: 10,
            Search: new string('a', ValidationConstants.DeviceSearchMaxLength + 1)
        );

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Search);
    }
}