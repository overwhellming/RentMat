using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Commands.Authentication;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Authentication;
using RentMat.Application.IntegrationTests.Infrastructure;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Authentication;

[Collection("Integration Tests Collection")]
public class RegisterCommandHandlerTests : BaseIntegrationTest
{
    private const string Login = "John";
    private const string Email = "john@test.com";
    private const string Password = "Password123";

    private readonly RegisterCommandHandler _commandHandler;
    private readonly IFusionCache _cache;

    public RegisterCommandHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _commandHandler = scope.ServiceProvider.GetRequiredService<RegisterCommandHandler>();
    }

    [Fact]
    public async Task Should_Create_In_Database_And_Return_JwtToken_When_Credentials_Are_Valid()
    {
        var command = new RegisterCommand(Login, Email, Password);
        var response = await _commandHandler.Handle(command, CancellationToken.None);

        response.AccessToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();

        var userInDb = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
        userInDb.Should().NotBeNull();
        userInDb.Login.Should().Be(command.Login);
        
        var refreshTokenInDb = await DbContext.RefreshTokenEntries
            .FirstOrDefaultAsync(t => t.Token == response.RefreshToken);
        refreshTokenInDb.Should().NotBeNull();
        refreshTokenInDb.UserId.Should().Be(userInDb.Id);
        refreshTokenInDb.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Throw_UserAlreadyExistsException_When_UserExists()
    {
        var command = new RegisterCommand(Login, Email, Password);
        await CreateUserAsync(Login, Email,  password: Password);
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _commandHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Should_Throw_UserAlreadyExistsException_When_TrimmedEmail_AlreadyExists()
    {
        await CreateUserAsync("Mark", Email,  password: Password);
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _commandHandler.Handle(new RegisterCommand(Login, Email + " ", Password), CancellationToken.None));
    }

    [Fact]
    public async Task Should_Throw_UserAlreadyExistsException_When_TrimmedLogin_AlreadyExists()
    {
        await CreateUserAsync(Login, Email, password: Password);
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _commandHandler.Handle(new RegisterCommand(Login + " ", Email, Password), CancellationToken.None));
    }

    [Fact]
    public async Task Should_InvalidateCache_When_UserRegistered()
    {
        const string key = "key";
        const string value = "data";
        await _cache.SetAsync(key, value, tags: ["users"]);

        await _commandHandler.Handle(new RegisterCommand(Login, Email, Password), CancellationToken.None);
        var cachedValue = await _cache.TryGetAsync<string>(key);
        cachedValue.HasValue.Should().BeFalse();
    }
}