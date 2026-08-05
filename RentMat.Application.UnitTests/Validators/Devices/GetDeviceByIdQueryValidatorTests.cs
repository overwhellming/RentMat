using FluentValidation.TestHelper;
using RentMat.Application.Queries.Devices;
using RentMat.Application.Validators.Devices;

namespace RentMat.Application.UnitTests.Validators.Devices;

public class GetDeviceByIdQueryValidatorTests
{
    private readonly GetDeviceByIdQueryValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Query_Is_Correct()
    {
        var query = new GetDeviceByIdQuery
        (
            1
        );

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_DeviceId_Is_Zero()
    {
        var query = new GetDeviceByIdQuery
        (
            0
        );

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}