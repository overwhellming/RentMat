using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Application.Handlers.Authentication;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Services.Interfaces;

namespace RentMat.Application.IntegrationTests.Handlers.Authentication;

[CollectionDefinition("Integration Tests Collection")]
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
    public async Task Should_ReturnJwtToken_WhenCredentials_Are_Valid()
    {
        var user = await CreateUserAsync(new RegisterDto("john", "john@test.com", CorrectPassword));
        var dto = new LoginDto(user.Login, CorrectPassword);

        var token = await _handler.Handle(dto, CancellationToken.None);
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_ThrowInvalidCredentialsException_WhenUserDoesNotExist()
    {
        var dto = new LoginDto("john", CorrectPassword);
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => 
            _handler.Handle(dto, CancellationToken.None));
    }

    [Fact]
    public async Task Should_ThrowInvalidCredentialsException_WhenPassword_Is_Invalid()
    {
        await CreateUserAsync(new RegisterDto("john", "john@test.com", CorrectPassword));
        var dto = new LoginDto("john", "incorrectpassword");
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => 
            _handler.Handle(dto, CancellationToken.None));
    }
}