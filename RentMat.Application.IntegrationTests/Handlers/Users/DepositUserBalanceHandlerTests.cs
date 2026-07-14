using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;

namespace RentMat.Application.IntegrationTests.Handlers.Users;

[CollectionDefinition("Integration Test Collection")]
public class DepositUserBalanceHandlerTests : BaseIntegrationTest
{
    private const decimal DepositAmount = 100;
    private readonly DepositUserBalanceHandler _handler;

    public DepositUserBalanceHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _handler = new DepositUserBalanceHandler(DbContext);
    }

    [Fact]
    public async Task Should_IncreaseBalance_And_CreateDepositRecord()
    {
        var user = await CreateUserAsync();
        var initialBalance = user.Balance;

        var response = await _handler.Handle(DepositAmount, user.Id, CancellationToken.None);

        DbContext.ChangeTracker.Clear();
        var updatedUser = (await DbContext.Users.FindAsync(user.Id));
        updatedUser.Should().NotBeNull();
        updatedUser.Balance.Should().Be(initialBalance + DepositAmount);

        response.Amount.Should().Be(DepositAmount);
        response.CurrentBalance.Should().Be(updatedUser.Balance);
        response.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));

        var depositRecord = await DbContext.Deposits.FindAsync(response.Id);
        depositRecord.Should().NotBeNull();
        depositRecord.Amount.Should().Be(DepositAmount);
        depositRecord.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Should_ThrowUserNotFoundException_When_UserDoesNotExist()
    {
        const int notExistingUserId = 1;
        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _handler.Handle(DepositAmount, notExistingUserId, CancellationToken.None));
    }
}