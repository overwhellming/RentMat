using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Commands.Users;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Core.Models;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Users;

[Collection("Integration Tests Collection")]
public class DepositUserBalanceCommandHandlerTests : BaseIntegrationTest
{
    private readonly DepositUserBalanceCommandHandler _commandHandler;
    private readonly IFusionCache _cache;
    
    public DepositUserBalanceCommandHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _commandHandler = new DepositUserBalanceCommandHandler(DbContext, _cache);
    }

    [Fact]
    public async Task Should_IncreaseBalance_And_CreateDepositRecord()
    {
        const decimal depositAmount = 100;
        
        var user = await CreateUserAsync();
        var initialBalance = user.Balance;

        var response = await _commandHandler.Handle(new DepositUserBalanceCommand(depositAmount, user.Id), CancellationToken.None);

        DbContext.ChangeTracker.Clear();
        var updatedUser = (await DbContext.Users.FindAsync(user.Id));
        updatedUser.Should().NotBeNull();
        updatedUser.Balance.Should().Be(initialBalance + depositAmount);

        response.Amount.Should().Be(depositAmount);
        response.CurrentBalance.Should().Be(updatedUser.Balance);
        response.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));

        var depositRecord = await DbContext.Deposits.FindAsync(response.Id);
        depositRecord.Should().NotBeNull();
        depositRecord.Amount.Should().Be(depositAmount);
        depositRecord.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Should_Throw_UserNotFoundException_When_UserDoesNotExist()
    {
        const decimal depositAmount = 100;
        const int notExistingId = 999;
        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _commandHandler.Handle(new DepositUserBalanceCommand(depositAmount, notExistingId), CancellationToken.None));
    }

    [Fact]
    public async Task Should_InvalidateCache_After_Deposit()
    {
        const decimal depositAmount = 100;
        var user = await CreateUserAsync();

        const string cacheKey = "key";
        await _cache.SetAsync(cacheKey, "test", tags: [CacheTags.Users]);
        var cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeTrue();

        await _commandHandler.Handle(new DepositUserBalanceCommand(depositAmount, user.Id), CancellationToken.None);
        cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeFalse();
    }
}