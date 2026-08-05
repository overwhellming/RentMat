using FluentValidation.TestHelper;
using RentMat.Application.Commands.Booking;
using RentMat.Application.Validators.Booking;

namespace RentMat.Application.UnitTests.Validators.Booking;

public class CompleteBookingCommandValidatorTests
{
    private readonly CompleteBookingCommandValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Command_Is_Correct()
    {
        var command = new CompleteBookingCommand
        (
            1,
            1
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_BookingId_Is_Zero()
    {
        var command = new CompleteBookingCommand
        (
            0,
            1
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.BookingId);
    }

    [Fact]
    public void Should_HaveError_When_UserId_Is_Zero()
    {
        var command = new CompleteBookingCommand
        (
            1,
            0
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}