using FluentValidation;
using RentMat.Application.DTOs.Device;
using RentMat.Core.Constants;

namespace RentMat.Application.Validators.Devices;

public class DeviceUpdateDtoValidator : AbstractValidator<DeviceUpdateDto>
{
    public DeviceUpdateDtoValidator()
    {
        RuleFor(d => d.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(ValidationConstants.DeviceNameMaxLength);
        
        RuleFor(d => d.HourRentPrice)
            .GreaterThan(0);
        
        RuleFor(d => d.CategoryId)
            .GreaterThan(0);
    }
}