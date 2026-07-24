using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Authentication;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Services.Interfaces;
using RentMat.Core.Models;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Authentication;

[Collection("Integration Tests Collection")]
public class RegisterHandlerTests : BaseIntegrationTest
{
    private const string Login = "John";
    private const string Email = "john@test.com";
    private const string Password = "Password123";

    private readonly RegisterHandler _handler;
    private readonly IFusionCache _cache;

    public RegisterHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();

        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        
        _handler = new RegisterHandler(DbContext,
            _cache,
            tokenService,
            new LoginHandler(DbContext,tokenService));
    }

    [Fact]
    public async Task Should_Create_In_Database_And_Return_JwtToken_When_Credentials_Are_Valid()
    {
        var dto = new RegisterDto(Login, Email, Password);
        var response = await _handler.Handle(dto, CancellationToken.None);

        response.AccessToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();

        var userInDb = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        userInDb.Should().NotBeNull();
        userInDb.Login.Should().Be(dto.Login);
        
        var refreshTokenInDb = await DbContext.RefreshTokenEntries
            .FirstOrDefaultAsync(t => t.Token == response.RefreshToken);
        refreshTokenInDb.Should().NotBeNull();
        refreshTokenInDb.UserId.Should().Be(userInDb.Id);
        refreshTokenInDb.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Throw_UserAlreadyExistsException_When_UserExists()
    {
        var dto = new RegisterDto(Login, Email, Password);
        await CreateUserAsync(Login, Email,  password: Password);
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _handler.Handle(dto, CancellationToken.None));
    }

    [Fact]
    public async Task Should_Throw_UserAlreadyExistsException_When_TrimmedEmail_AlreadyExists()
    {
        await CreateUserAsync("Mark", Email,  password: Password);
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _handler.Handle(new RegisterDto(Login, Email + " ", Password), CancellationToken.None));
    }

    [Fact]
    public async Task Should_Throw_UserAlreadyExistsException_When_TrimmedLogin_AlreadyExists()
    {
        await CreateUserAsync(Login, Email, password: Password);
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _handler.Handle(new RegisterDto(Login + " ", Email, Password), CancellationToken.None));
    }

    [Fact]
    public async Task Should_InvalidateCache_When_UserRegistered()
    {
        const string key = "key";
        const string value = "data";
        await _cache.SetAsync(key, value, tags: ["users"]);

        await _handler.Handle(new RegisterDto(Login, Email, Password), CancellationToken.None);
        var cachedValue = await _cache.TryGetAsync<string>(key);
        cachedValue.HasValue.Should().BeFalse();
    }
}