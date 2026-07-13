using FluentValidation.TestHelper;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Validators.Authentication;
using RentMat.Core.Constants;

namespace RentMat.Application.UnitTests.Validators.Authentication;

public class RegisterDtoValidatorTests
{
    private readonly RegisterDtoValidator _validator = new();

    [Fact]
    public void ShouldBeValid_When_Dto_Is_Correct()
    {
        var dto = new RegisterDto
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_When_Email_Is_Empty()
    {
        var dto = new RegisterDto
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: string.Empty,
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [Fact]
    public void ShouldHaveError_When_Email_Is_Invalid()
    {
        var dto = new RegisterDto
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: "test@",
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [Fact]
    public void ShouldHaveError_When_Login_Is_Empty()
    {
        var dto = new RegisterDto
        (
            Login: string.Empty,
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }
    
    [Fact]
    public void ShouldHaveError_When_Login_Contains_Whitespaces()
    {
        var dto = new RegisterDto
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength) + ' ',
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }
    
    [Fact]
    public void ShouldHaveError_When_Login_Exceed_MaximumLength()
    {
        var dto = new RegisterDto
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength + 1),
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMinLength)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Login);
    }
    
    [Fact]
    public void ShouldHaveError_When_Password_Is_Empty()
    {
        var dto = new RegisterDto
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: "test@test.test",
            Password: string.Empty
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
    
    [Fact]
    public void ShouldHaveError_When_Password_Exceed_MaximumLength()
    {
        var dto = new RegisterDto
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMaxLength + 1)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
    
    [Fact]
    public void ShouldHaveError_When_Password_Is_Shorter_Than_MinimumLength()
    {
        var dto = new RegisterDto
        (
            Login: new string('a', ValidationConstants.UserLoginMaxLength),
            Email: "test@test.test",
            Password: new string('a', ValidationConstants.UserPasswordMinLength - 1)
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}