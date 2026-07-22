using FluentAssertions;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;

namespace RentMat.Application.IntegrationTests.Handlers.Users;

[Collection("Integration Tests Collection")]
public class GetUserBalanceHandlerTests : BaseIntegrationTest
{
    private readonly GetUserBalanceHandler _handler;

    public GetUserBalanceHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _handler = new GetUserBalanceHandler(DbContext);
    }

    [Fact]
    public async Task Should_Return_UserBalance()
    {
        const decimal balance = 100;
        var user = await CreateUserAsync(balance: balance);
        var response = await _handler.Handle(user.Id, CancellationToken.None);
        response.Should().Be(balance);
    }

    [Fact]
    public async Task Should_Throw_UserNotFoundException_When_UserDoesNotExist()
    {
        const int notExistingId = 999;
        await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(notExistingId, CancellationToken.None));
    }
}