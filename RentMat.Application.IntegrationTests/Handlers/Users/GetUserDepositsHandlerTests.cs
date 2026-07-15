using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Core.Models;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Users;

[CollectionDefinition("Integration Test Collection")]
public class GetUserDepositsHandlerTests : BaseIntegrationTest
{
    private readonly GetUserDepositsHandler _handler;
    
    private readonly DepositUserBalanceHandler _depositHandler;
    private readonly IFusionCache _cache;
    
    public GetUserDepositsHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _handler = new GetUserDepositsHandler(DbContext);
        
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _depositHandler = new DepositUserBalanceHandler(DbContext, _cache);
    }
    
    [Fact]
    public async Task Should_Return_UserDeposits()
    {
        const decimal depositAmount = 100;
        var user = await CreateUserAsync();
        var createdDeposit = await _depositHandler.Handle(depositAmount, user.Id, CancellationToken.None);

        var response = await _handler.Handle(user.Id, CancellationToken.None);
        response.Should().HaveCount(1);
        response.Select(d => d.Id).Should().Contain(createdDeposit.Id);
    }

    [Fact]
    public async Task Should_Return_EmptyList_When_UserDoesNotExist()
    {
        const int notExistingId = 999;
        var response = await _handler.Handle(notExistingId, CancellationToken.None);
        response.Should().HaveCount(0);
    }
}