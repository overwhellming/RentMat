using FluentAssertions;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Queries.Users;

namespace RentMat.Application.IntegrationTests.Handlers.Users;

[Collection("Integration Tests Collection")]
public class GetUserBalanceQueryHandlerTests : BaseIntegrationTest
{
    private readonly GetUserBalanceQueryHandler _queryHandler;

    public GetUserBalanceQueryHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _queryHandler = new GetUserBalanceQueryHandler(DbContext);
    }

    [Fact]
    public async Task Should_Return_UserBalance()
    {
        const decimal balance = 100;
        var user = await CreateUserAsync(balance: balance);
        var response = await _queryHandler.Handle(new GetUserBalanceQuery(user.Id), CancellationToken.None);
        response.Should().Be(balance);
    }

    [Fact]
    public async Task Should_Throw_UserNotFoundException_When_UserDoesNotExist()
    {
        const int notExistingId = 999;
        await Assert.ThrowsAsync<UserNotFoundException>(() => _queryHandler.Handle(new GetUserBalanceQuery(notExistingId), CancellationToken.None));
    }
}