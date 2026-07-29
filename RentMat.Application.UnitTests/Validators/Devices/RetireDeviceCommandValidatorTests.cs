using FluentValidation.TestHelper;
using RentMat.Application.Commands.Devices;
using RentMat.Application.Queries.Devices;
using RentMat.Application.Validators.Devices;

namespace RentMat.Application.UnitTests.Validators.Devices;

public class RetireDeviceCommandValidatorTests
{
    private readonly RetireDeviceCommandValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Dto_Is_Correct()
    {
        var command = new RetireDeviceCommand
        (
            Id: 1
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Should_HaveError_When_DeviceId_Is_Zero()
    {
        var command = new RetireDeviceCommand
        (
            Id: 0
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}