using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Commands.Users;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Queries.Users;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Users;

[Collection("Integration Tests Collection")]
public class GetUserDepositsQueryHandlerTests : BaseIntegrationTest
{
    private readonly IFusionCache _cache;

    private readonly DepositUserBalanceCommandHandler _depositCommandHandler;
    private readonly GetUserDepositsQueryHandler _queryHandler;

    public GetUserDepositsQueryHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _queryHandler = new GetUserDepositsQueryHandler(DbContext);

        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _depositCommandHandler = new DepositUserBalanceCommandHandler(DbContext, _cache);
    }

    [Fact]
    public async Task Should_Return_UserDeposits()
    {
        const decimal depositAmount = 100;
        var user = await CreateUserAsync();
        var createdDeposit = await _depositCommandHandler.Handle(new DepositUserBalanceCommand(depositAmount, user.Id),
            CancellationToken.None);

        var response = await _queryHandler.Handle(new GetUserDepositsQuery(user.Id), CancellationToken.None);
        response.Should().HaveCount(1);
        response.Select(d => d.Id).Should().Contain(createdDeposit.Id);
    }

    [Fact]
    public async Task Should_Return_EmptyList_When_UserDoesNotExist()
    {
        const int notExistingId = 999;
        var response = await _queryHandler.Handle(new GetUserDepositsQuery(notExistingId), CancellationToken.None);
        response.Should().HaveCount(0);
    }
}