using FluentValidation.TestHelper;
using RentMat.Application.Commands.Devices;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Validators.Devices;
using RentMat.Core.Constants;

namespace RentMat.Application.UnitTests.Validators.Devices;

public class CreateDeviceCommandValidatorTests
{
    private readonly CreateDeviceCommandValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Dto_Is_Correct()
    {
        var command = new CreateDeviceCommand
        (
            Name: new string('a', ValidationConstants.DeviceNameMaxLength),
            HourRentPrice: 1000,
            CategoryId: 1
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_Name_Is_Empty()
    {
        var command = new CreateDeviceCommand
        (
            Name: string.Empty,
            HourRentPrice: 1000,
            CategoryId: 1
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public void Should_HaveError_When_Name_Exceed_MaximumLength()
    {
        var command = new CreateDeviceCommand
        (
            Name: new string('a', ValidationConstants.DeviceNameMaxLength + 1),
            HourRentPrice: 1000,
            CategoryId: 1
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public void Should_HaveError_When_HourRentPrice_Is_Zero()
    {
        var command = new CreateDeviceCommand
        (
            Name: new string('a', ValidationConstants.DeviceNameMaxLength),
            HourRentPrice: 0,
            CategoryId: 1
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.HourRentPrice);
    }
    
    [Fact]
    public void Should_HaveError_When_CategoryId_Is_Zero()
    {
        var command = new CreateDeviceCommand
        (
            Name: new string('a', ValidationConstants.DeviceNameMaxLength),
            HourRentPrice: 1000,
            CategoryId: 0
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }
}