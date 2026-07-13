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

[CollectionDefinition("Integration Tests Collection")]
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
        _handler = new RegisterHandler(DbContext,
            _cache,
            scope.ServiceProvider.GetRequiredService<IJwtTokenService>());
    }

    [Fact]
    public async Task Should_ReturnJwtToken_WhenCredentials_Are_Valid()
    {
        var dto = new RegisterDto(Login, Email, Password);
        var token = await _handler.Handle(dto, CancellationToken.None);

        token.Should().NotBeNullOrEmpty();

        var userInDb = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        userInDb.Should().NotBeNull();
        userInDb.Login.Should().Be(dto.Login);
    }

    [Fact]
    public async Task Should_ThrowUserAlreadyExistsException_WhenUserExists()
    {
        var dto = new RegisterDto(Login, Email, Password);
        await CreateUserAsync(dto);
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _handler.Handle(dto, CancellationToken.None));
    }

    [Fact]
    public async Task Should_ThrowUserAlreadyExistsException_WhenTrimmedEmail_AlreadyExists()
    {
        await CreateUserAsync(new RegisterDto("Mark", Email, Password));
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _handler.Handle(new RegisterDto(Login, Email + " ", Password), CancellationToken.None));
    }

    [Fact]
    public async Task Should_ThrowUserAlreadyExistsException_WhenTrimmedLogin_AlreadyExists()
    {
        await CreateUserAsync(new RegisterDto(Login, Email, Password));
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _handler.Handle(new RegisterDto(Login + " ", Email, Password), CancellationToken.None));
    }

    [Fact]
    public async Task Should_InvalidateCache_WhenUserRegistered()
    {
        const string key = "key";
        const string value = "data";
        await _cache.SetAsync(key, value, tags: ["users"]);

        await _handler.Handle(new RegisterDto(Login, Email, Password), CancellationToken.None);
        var cachedValue = await _cache.TryGetAsync<string>(key);
        cachedValue.HasValue.Should().BeFalse();
    }
}