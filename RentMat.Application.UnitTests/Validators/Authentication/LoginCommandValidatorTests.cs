using FluentValidation.TestHelper;
using RentMat.Application.Commands.Authentication;
using RentMat.Application.Validators.Authentication;
using RentMat.Core.Constants;

namespace RentMat.Application.UnitTests.Validators.Authentication;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Command_Is_Correct()
    {
        var command = new LoginCommand
        (
            new string('a', ValidationConstants.UserLoginMaxLength),
            new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_Login_Is_Empty()
    {
        var command = new LoginCommand
        (
            string.Empty,
            new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }

    [Fact]
    public void Should_HaveError_When_Login_Contains_Whitespaces()
    {
        var command = new LoginCommand
        (
            new string('a', ValidationConstants.UserLoginMaxLength) + ' ',
            new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }

    [Fact]
    public void Should_HaveError_When_Login_Exceed_MaximumLength()
    {
        var command = new LoginCommand
        (
            new string('a', ValidationConstants.UserLoginMaxLength + 1),
            new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }

    [Fact]
    public void Should_HaveError_When_Password_Is_Empty()
    {
        var command = new LoginCommand
        (
            new string('a', ValidationConstants.UserLoginMaxLength),
            string.Empty
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_HaveError_When_Password_Exceed_MaximumLength()
    {
        var command = new LoginCommand
        (
            new string('a', ValidationConstants.UserLoginMaxLength),
            new string('a', ValidationConstants.UserPasswordMaxLength + 1)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_HaveError_When_Password_Is_Shorter_Than_MinimumLength()
    {
        var command = new LoginCommand
        (
            new string('a', ValidationConstants.UserLoginMaxLength),
            new string('a', ValidationConstants.UserPasswordMinLength - 1)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}