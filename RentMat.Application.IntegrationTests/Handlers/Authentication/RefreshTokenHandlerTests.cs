using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Application.Handlers.Authentication;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Services;
using RentMat.Application.Services.Interfaces;
using RentMat.Core.Models;

namespace RentMat.Application.IntegrationTests.Handlers.Authentication;

[Collection("Integration Tests Collection")]
public class RefreshTokenHandlerTests : BaseIntegrationTest
{
    private readonly RefreshTokenHandler _handler;

    public RefreshTokenHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _handler = new RefreshTokenHandler(DbContext, scope.ServiceProvider.GetRequiredService<IJwtTokenService>());
    }

    [Fact]
    public async Task Should_Update_RefreshToken_And_Create_NewEntry()
    {
        var initialResponse = await RegisterUserAsync();
        var dto = new RefreshTokenDto(initialResponse.AccessToken, initialResponse.RefreshToken);
        
        var refreshedResponse = await _handler.Handle(dto, CancellationToken.None);

        refreshedResponse.Should().NotBeNull();
        refreshedResponse.AccessToken.Should().NotBe(initialResponse.AccessToken);
        refreshedResponse.RefreshToken.Should().NotBe(initialResponse.RefreshToken);

        var newEntryInDb =
            await DbContext.RefreshTokenEntries.FirstOrDefaultAsync(e => e.Token == refreshedResponse.RefreshToken);
        newEntryInDb.Should().NotBeNull();
        newEntryInDb.Token.Should().Be(refreshedResponse.RefreshToken);
        newEntryInDb.IsRevoked.Should().BeFalse();
        newEntryInDb.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        newEntryInDb.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(JwtTokenService.RefreshTokenDays),
            TimeSpan.FromSeconds(1));

        var oldEntryInDb =
            await DbContext.RefreshTokenEntries.FirstOrDefaultAsync(e => e.Token == initialResponse.RefreshToken);
        oldEntryInDb.Should().NotBeNull();
        oldEntryInDb.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Throw_InvalidAccessTokenException_When_PrincipalNotFoundFromToken()
    {
        const string invalidAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30";
        const string invalidRefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.k2lP6x9T4N7M0cBeO6r-ije4IPWW2KEL0JvUXerWp2k";

        var tokenServiceMock = new Mock<IJwtTokenService>();
        tokenServiceMock
            .Setup(s => s.GetPrincipalFromExpiredToken(invalidAccessToken))
            .Returns((ClaimsPrincipal?)null);
        
        var dto = new RefreshTokenDto(invalidAccessToken, invalidRefreshToken);
        await Assert.ThrowsAsync<InvalidAccessTokenException>(() =>
            new RefreshTokenHandler(DbContext, tokenServiceMock.Object).Handle(dto, CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_Throw_InvalidTokenClaimsException_When_SubClaimIsMissing()
    {
        const string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30";
        const string refreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.k2lP6x9T4N7M0cBeO6r-ije4IPWW2KEL0JvUXerWp2k";

        var tokenServiceMock = new Mock<IJwtTokenService>();
    
        var identity = new ClaimsIdentity(); 
        var principal = new ClaimsPrincipal(identity);

        tokenServiceMock
            .Setup(s => s.GetPrincipalFromExpiredToken(accessToken))
            .Returns(principal);
    
        var dto = new RefreshTokenDto(accessToken, refreshToken);
    
        await Assert.ThrowsAsync<InvalidTokenClaimsException>(() =>
            new RefreshTokenHandler(DbContext, tokenServiceMock.Object).Handle(dto, CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_Throw_InvalidRefreshTokenException_When_TokenNotFoundInDatabase()
    {
        const string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30";
        const string notExistingRefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.k2lP6x9T4N7M0cBeO6r-ije4IPWW2KEL0JvUXerWp2k";

        var tokenServiceMock = new Mock<IJwtTokenService>();
    
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        tokenServiceMock
            .Setup(s => s.GetPrincipalFromExpiredToken(accessToken))
            .Returns(principal);
    
        var dto = new RefreshTokenDto(accessToken, notExistingRefreshToken);
    
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() =>
            new RefreshTokenHandler(DbContext, tokenServiceMock.Object).Handle(dto, CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_Throw_InvalidRefreshTokenException_When_TokenIsRevoked()
    {
        const int userId = 1;
        const string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30";
        const string revokedRefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.k2lP6x9T4N7M0cBeO6r-ije4IPWW2KEL0JvUXerWp2k";

        var tokenServiceMock = new Mock<IJwtTokenService>();
    
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        tokenServiceMock
            .Setup(s => s.GetPrincipalFromExpiredToken(accessToken))
            .Returns(principal);

        DbContext.RefreshTokenEntries.Add(new RefreshTokenEntry
        {
            Token = revokedRefreshToken,
            UserId = userId,
            IsRevoked = true,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        });
        
        var dto = new RefreshTokenDto(accessToken, revokedRefreshToken);
    
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() =>
            new RefreshTokenHandler(DbContext, tokenServiceMock.Object).Handle(dto, CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_Throw_InvalidRefreshTokenException_When_TokenIsExpired()
    {
        const int userId = 1;
        const string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30";
        const string revokedRefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.k2lP6x9T4N7M0cBeO6r-ije4IPWW2KEL0JvUXerWp2k";

        var tokenServiceMock = new Mock<IJwtTokenService>();
    
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        tokenServiceMock
            .Setup(s => s.GetPrincipalFromExpiredToken(accessToken))
            .Returns(principal);

        DbContext.RefreshTokenEntries.Add(new RefreshTokenEntry
        {
            Token = revokedRefreshToken,
            UserId = userId,
            IsRevoked = false,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1),
            CreatedAt = DateTimeOffset.UtcNow
        });
        
        var dto = new RefreshTokenDto(accessToken, revokedRefreshToken);
    
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() =>
            new RefreshTokenHandler(DbContext, tokenServiceMock.Object).Handle(dto, CancellationToken.None));
    }
}