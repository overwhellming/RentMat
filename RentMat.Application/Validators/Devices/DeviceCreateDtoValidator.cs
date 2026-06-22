using FluentValidation;
using RentMat.Application.DTOs.Device;
using RentMat.Core.Constants;

namespace RentMat.Application.Validators.Devices;

public class DeviceCreateDtoValidator : AbstractValidator<DeviceCreateDto>
{
    public DeviceCreateDtoValidator()
    {
        RuleFor(d => d.Name)
            .MaximumLength(ValidationConstants.DeviceNameMaxLength);
    }
}