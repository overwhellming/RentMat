using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Application.Handlers.Authentication;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Services.Interfaces;

namespace RentMat.Application.IntegrationTests.Handlers.Authentication;

[Collection("Integration Tests Collection")]
public class LoginHandlerTests : BaseIntegrationTest
{
    private const string CorrectPassword = "Password123";
    private readonly LoginHandler _handler;

    public LoginHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _handler = new LoginHandler(DbContext, scope.ServiceProvider.GetRequiredService<IJwtTokenService>());
    }

    [Fact]
    public async Task Should_Create_In_Database_And_Return_JwtToken_When_Credentials_Are_Valid()
    {
        var user = await CreateUserAsync(password: CorrectPassword);
        var dto = new LoginDto(user.Login, CorrectPassword);

        var token = await _handler.Handle(dto, CancellationToken.None);
        token.AccessToken.Should().NotBeNullOrEmpty();
        token.RefreshToken.Should().NotBeNullOrEmpty();
        
        var tokenInDb = await DbContext.RefreshTokenEntries.FirstOrDefaultAsync(e => e.Token == token.RefreshToken);
        tokenInDb.Should().NotBeNull();
        tokenInDb.UserId.Should().Be(user.Id);
        tokenInDb.IsRevoked.Should().BeFalse();
        tokenInDb.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
        tokenInDb.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Should_Create_New_RefreshToken_On_Every_Login()
    {
        var user = await CreateUserAsync(password: CorrectPassword);
        var dto = new LoginDto(user.Login, CorrectPassword);

        var firstToken = await _handler.Handle(dto, CancellationToken.None);
        var secondToken = await _handler.Handle(dto, CancellationToken.None);

        firstToken.Should().NotBe(secondToken);
        var tokensInDb = await DbContext.RefreshTokenEntries.Where(e => e.UserId == user.Id)
            .ToListAsync();
        tokensInDb.Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_Throw_InvalidCredentialsException_When_UserDoesNotExist()
    {
        var dto = new LoginDto("john", CorrectPassword);
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _handler.Handle(dto, CancellationToken.None));
    }

    [Fact]
    public async Task Should_Throw_InvalidCredentialsException_When_Password_Is_Invalid()
    {
        await CreateUserAsync(password: CorrectPassword);
        var dto = new LoginDto("john", "incorrectpassword");
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _handler.Handle(dto, CancellationToken.None));
    }
}