using FluentValidation.TestHelper;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.Queries.Devices;
using RentMat.Application.Validators.Devices;
using RentMat.Core.Constants;

namespace RentMat.Application.UnitTests.Validators.Devices;

public class GetAllDevicesQueryValidatorTests
{
    private readonly GetAllDevicesQueryValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Query_Is_Correct()
    {
        var query = new GetAllDevicesQuery
        (
            1,
            10
        );

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_Page_Is_Zero()
    {
        var query = new GetAllDevicesQuery
        (
            0,
            10
        );

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Should_HaveError_When_PageSize_Is_Zero()
    {
        var query = new GetAllDevicesQuery
        (
            1,
            0
        );

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Should_HaveError_When_PageSize_Exceed_MaxPageSize()
    {
        var query = new GetAllDevicesQuery
        (
            1,
            GetAllDevicesQueryHandler.MaxPageSize + 1
        );

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Should_HaveError_When_Search_Exceed_MaxLength()
    {
        var query = new GetAllDevicesQuery
        (
            1,
            10,
            new string('a', ValidationConstants.DeviceSearchMaxLength + 1)
        );

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Search);
    }
}