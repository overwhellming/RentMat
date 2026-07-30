using FluentValidation.TestHelper;
using RentMat.Application.Commands.Authentication;
using RentMat.Application.Validators.Authentication;

namespace RentMat.Application.UnitTests.Validators.Authentication;

public class RevokeRefreshTokenCommandValidatorTests
{
    private readonly RevokeRefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Command_Is_Correct()
    {
        var command = new RevokeRefreshTokenCommand
        (
            UserId: 1
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_RefreshToken_Is_Zero()
    {
        var command = new RevokeRefreshTokenCommand
        (
            UserId: 0
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}