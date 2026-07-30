using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Commands.Authentication;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Application.Handlers.Authentication;
using RentMat.Application.IntegrationTests.Infrastructure;

namespace RentMat.Application.IntegrationTests.Handlers.Authentication;

[Collection("Integration Tests Collection")]
public class RevokeRefreshTokenCommandHandlerTests : BaseIntegrationTest
{
    private readonly RevokeRefreshTokenCommandHandler _commandHandler;
    
    public RevokeRefreshTokenCommandHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _commandHandler = new RevokeRefreshTokenCommandHandler(DbContext);
    }

    [Fact]
    public async Task Should_Revoke_RefreshToken_In_Database()
    {
        var tokenResponse = await RegisterUserAsync();
        
        var initialTokenEntry =
            await DbContext.RefreshTokenEntries.FirstOrDefaultAsync(e => e.Token == tokenResponse.RefreshToken);
        initialTokenEntry.Should().NotBeNull();
        initialTokenEntry.IsRevoked.Should().BeFalse();
        
        DbContext.ChangeTracker.Clear();
        
        await _commandHandler.Handle(new RevokeRefreshTokenCommand(initialTokenEntry.UserId), CancellationToken.None);
        var revokedTokenEntry =
            await DbContext.RefreshTokenEntries.FirstOrDefaultAsync(e => e.Token == tokenResponse.RefreshToken);
        revokedTokenEntry.Should().NotBeNull();
        revokedTokenEntry.IsRevoked.Should().BeTrue();
    }
    [Fact]
    public async Task Should_Throw_ActiveRefreshTokenNotFoundException_When_UserDoesNotExist()
    {
        const int notExistingUserId = 999;
    
        await Assert.ThrowsAsync<ActiveRefreshTokenNotFoundException>(() =>
            _commandHandler.Handle(new RevokeRefreshTokenCommand(notExistingUserId), CancellationToken.None));
    }

    [Fact]
    public async Task Should_Throw_ActiveRefreshTokenNotFoundException_When_UserHasNoActiveTokens()
    {
        var user = await CreateUserAsync();
    
        await Assert.ThrowsAsync<ActiveRefreshTokenNotFoundException>(() =>
            _commandHandler.Handle(new RevokeRefreshTokenCommand(user.Id), CancellationToken.None));
    }
}