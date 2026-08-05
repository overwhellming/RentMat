using FluentValidation.TestHelper;
using RentMat.Application.Commands.Users;
using RentMat.Application.Validators.Users;
using RentMat.Core.Constants;

namespace RentMat.Application.UnitTests.Validators.Users;

public class DepositUserBalanceCommandValidatorTests
{
    private readonly DepositUserBalanceCommandValidator _validator = new();

    [Fact]
    public void Should_BeValid_When_Command_Is_Correct()
    {
        var command = new DepositUserBalanceCommand
        (
            Amount: 100,
            UserId: 1
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_Amount_Is_Zero()
    {
        var command = new DepositUserBalanceCommand
        (
            Amount: 0,
            UserId: 1
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_HaveError_When_Amount_Exceed_MaxDepositAmount()
    {
        var command = new DepositUserBalanceCommand
        (
            Amount: ValidationConstants.MaxDepositAmount + 1,
            UserId: 1
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }
    
    [Fact]
    public void Should_HaveError_When_UserId_Is_Zero()
    {
        var command = new DepositUserBalanceCommand
        (
            Amount: 100,
            UserId: 0
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}