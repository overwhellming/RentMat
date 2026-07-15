using FluentValidation.TestHelper;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Application.Validators.Booking;

namespace RentMat.Application.UnitTests.Validators.Booking;

public class BookingCreateDtoValidatorTests
{
    private readonly BookingCreateDtoValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Dto_Is_Correct()
    {
        var dto = new BookingCreateDto
        (
            DeviceId: 1,
            StartDate: DateTimeOffset.UtcNow.AddDays(1),
            EndDate: DateTimeOffset.UtcNow.AddDays(2)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_Id_Is_Zero()
    {
        var dto = new BookingCreateDto
        (
            DeviceId: 0,
            StartDate: DateTimeOffset.UtcNow.AddDays(1),
            EndDate: DateTimeOffset.UtcNow.AddDays(2)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DeviceId);
    }

    [Fact]
    public void Should_HaveError_When_StartDate_Is_In_Past()
    {
        var dto = new BookingCreateDto
        (
            DeviceId: 1,
            StartDate: DateTimeOffset.UtcNow.AddDays(-1),
            EndDate: DateTimeOffset.UtcNow.AddDays(2)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("Start date cannot be in the past");
    }

    [Fact]
    public void Should_HaveError_When_StartDate_Is_After_EndDate()
    {
        var dto = new BookingCreateDto
        (
            DeviceId: 1,
            StartDate: DateTimeOffset.UtcNow.AddDays(5),
            EndDate: DateTimeOffset.UtcNow.AddDays(2)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("Start date must be before the end date");
    }
}