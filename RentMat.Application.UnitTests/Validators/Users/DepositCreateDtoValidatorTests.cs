using FluentValidation.TestHelper;
using RentMat.Application.DTOs.User;
using RentMat.Application.Validators.Users;

namespace RentMat.Application.UnitTests.Validators.Users;

public class DepositCreateDtoValidatorTests
{
    private readonly DepositCreateDtoValidator _validator = new();

    [Fact]
    public void ShouldBeValid_When_Dto_Is_Correct()
    {
        var dto = new DepositCreateDto
        (
            Amount: 100
        );

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_When_Amount_Is_Zero()
    {
        var dto = new DepositCreateDto
        (
            Amount: 0
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }
}