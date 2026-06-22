using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RentMat.Application.DTOs.Device;
using RentMat.Core.Constants;

namespace RentMat.Application.Validators.Devices;

public class DeviceUpdateDtoValidator : AbstractValidator<DeviceUpdateDto>
{
    public DeviceUpdateDtoValidator()
    {
        RuleFor(d => d.Name)
            .MaximumLength(ValidationConstants.DeviceNameMaxLength);
    }
}