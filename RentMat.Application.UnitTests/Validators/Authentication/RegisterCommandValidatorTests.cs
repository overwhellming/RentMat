using FluentValidation.TestHelper;
using RentMat.Application.Commands.Authentication;
using RentMat.Application.Validators.Authentication;
using RentMat.Core.Constants;

namespace RentMat.Application.UnitTests.Validators.Authentication;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_command_Is_Correct()
    {
        var command = new RegisterCommand
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_Email_Is_Empty()
    {
        var command = new RegisterCommand
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: string.Empty,
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [Fact]
    public void Should_HaveError_When_Email_Is_Invalid()
    {
        var command = new RegisterCommand
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: "test@",
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [Fact]
    public void Should_HaveError_When_Login_Is_Empty()
    {
        var command = new RegisterCommand
        (
            Login: string.Empty,
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }
    
    [Fact]
    public void Should_HaveError_When_Login_Contains_Whitespaces()
    {
        var command = new RegisterCommand
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength) + ' ',
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }
    
    [Fact]
    public void Should_HaveError_When_Login_Exceed_MaximumLength()
    {
        var command = new RegisterCommand
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength + 1),
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }
    
    [Fact]
    public void Should_HaveError_When_Password_Is_Empty()
    {
        var command = new RegisterCommand
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: "test@test.test",
            Password: string.Empty
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
    
    [Fact]
    public void Should_HaveError_When_Password_Exceed_MaximumLength()
    {
        var command = new RegisterCommand
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMaxLength + 1)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
    
    [Fact]
    public void Should_HaveError_When_Password_Is_Shorter_Than_MinimumLength()
    {
        var command = new RegisterCommand
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMinLength - 1)
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}