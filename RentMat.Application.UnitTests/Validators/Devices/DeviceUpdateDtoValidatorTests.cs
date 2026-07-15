using FluentValidation.TestHelper;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Validators.Devices;
using RentMat.Core.Constants;

namespace RentMat.Application.UnitTests.Validators.Devices;

public class DeviceUpdateDtoValidatorTests
{
    private readonly DeviceUpdateDtoValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Dto_Is_Correct()
    {
        var dto = new DeviceUpdateDto
        (
            Name: new string('a', ValidationConstants.DeviceNameMaxLength),
            HourRentPrice: 1000,
            CategoryId: 1
        );

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_Name_Is_Empty()
    {
        var dto = new DeviceUpdateDto
        (
            Name: string.Empty,
            HourRentPrice: 1000,
            CategoryId: 1
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public void Should_HaveError_When_Name_Exceed_MaximumLength()
    {
        var dto = new DeviceUpdateDto
        (
            Name: new string('a', ValidationConstants.DeviceNameMaxLength + 1),
            HourRentPrice: 1000,
            CategoryId: 1
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public void Should_HaveError_When_HourRentPrice_Is_Zero()
    {
        var dto = new DeviceUpdateDto
        (
            Name: new string('a', ValidationConstants.DeviceNameMaxLength),
            HourRentPrice: 0,
            CategoryId: 1
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.HourRentPrice);
    }
    
    [Fact]
    public void Should_HaveError_When_CategoryId_Is_Zero()
    {
        var dto = new DeviceUpdateDto
        (
            Name: new string('a', ValidationConstants.DeviceNameMaxLength),
            HourRentPrice: 1000,
            CategoryId: 0
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }
}