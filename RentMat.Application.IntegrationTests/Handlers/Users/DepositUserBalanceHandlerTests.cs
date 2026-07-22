using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Core.Models;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Users;

[Collection("Integration Tests Collection")]
public class DepositUserBalanceHandlerTests : BaseIntegrationTest
{
    private readonly DepositUserBalanceHandler _handler;
    private readonly IFusionCache _cache;
    
    public DepositUserBalanceHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _handler = new DepositUserBalanceHandler(DbContext, _cache);
    }

    [Fact]
    public async Task Should_IncreaseBalance_And_CreateDepositRecord()
    {
        const decimal depositAmount = 100;
        
        var user = await CreateUserAsync();
        var initialBalance = user.Balance;

        var response = await _handler.Handle(depositAmount, user.Id, CancellationToken.None);

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
            _handler.Handle(depositAmount, notExistingId, CancellationToken.None));
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

        await _handler.Handle(depositAmount, user.Id, CancellationToken.None);
        cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeFalse();
    }
}