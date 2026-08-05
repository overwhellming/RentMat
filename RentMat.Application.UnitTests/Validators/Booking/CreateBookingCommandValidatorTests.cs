using FluentValidation.TestHelper;
using RentMat.Application.Commands.Booking;
using RentMat.Application.Validators.Booking;

namespace RentMat.Application.UnitTests.Validators.Booking;

public class CreateBookingCommandValidatorTests
{
    private readonly CreateBookingCommandValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Command_Is_Correct()
    {
        var command = new CreateBookingCommand
        (
            1,
            1,
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2)
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_DeviceId_Is_Zero()
    {
        var command = new CreateBookingCommand
        (
            0,
            1,
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DeviceId);
    }

    [Fact]
    public void Should_HaveError_When_UserId_Is_Zero()
    {
        var command = new CreateBookingCommand
        (
            1,
            0,
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_HaveError_When_StartDate_Is_In_Past()
    {
        var command = new CreateBookingCommand
        (
            1,
            1,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(2)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("Start date cannot be in the past");
    }

    [Fact]
    public void Should_HaveError_When_StartDate_Is_After_EndDate()
    {
        var command = new CreateBookingCommand
        (
            1,
            1,
            DateTimeOffset.UtcNow.AddDays(5),
            DateTimeOffset.UtcNow.AddDays(2)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("Start date must be before the end date");
    }
}