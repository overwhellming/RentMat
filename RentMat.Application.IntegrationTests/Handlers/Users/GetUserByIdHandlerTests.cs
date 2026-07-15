using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Users;

[CollectionDefinition("Integration Test Collection")]
public class GetUserByIdHandlerTests : BaseIntegrationTest
{
    private readonly GetUserByIdHandler _handler;
    private readonly IFusionCache _cache;
    
    public GetUserByIdHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _handler = new GetUserByIdHandler(DbContext, _cache,
            scope.ServiceProvider.GetRequiredService<ILogger<GetUserByIdHandler>>());
    }

    [Fact]
    public async Task Should_Return_User()
    {
        const string login = "Alexi";
        var user = await CreateUserAsync(login: login);
        var response = await _handler.Handle(user.Id, CancellationToken.None);
        response.Login.Should().Be(login);
    }

    [Fact]
    public async Task Should_Throw_UserNotFoundException_When_UserDoesNotExist()
    {
        const int notExistingId = 999;
        await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(notExistingId, CancellationToken.None));
    }

    [Fact]
    public async Task Should_Return_CachedUser_When_DataChanged()
    {
        const decimal initialBalance = 100;
        var user = await CreateUserAsync(balance: initialBalance);
        
        var response = await _handler.Handle(user.Id, CancellationToken.None);
        response.Balance.Should().Be(initialBalance);

        var userInDb = await DbContext.Users.FindAsync(user.Id);
        userInDb.Balance = initialBalance + 1;
        await DbContext.SaveChangesAsync();
        
        response = await _handler.Handle(user.Id, CancellationToken.None);
        response.Balance.Should().Be(initialBalance);
    }
    
    [Fact]
    public async Task Should_Return_UpdatedData_After_CacheInvalidation()
    {
        const decimal initialBalance = 100;
        var user = await CreateUserAsync(balance: initialBalance);
        
        var response = await _handler.Handle(user.Id, CancellationToken.None);
        response.Balance.Should().Be(initialBalance);

        var userInDb = await DbContext.Users.FindAsync(user.Id);
        userInDb.Balance = initialBalance + 1;
        await DbContext.SaveChangesAsync();
        await _cache.RemoveByTagAsync(CacheTags.Users);
        
        response = await _handler.Handle(user.Id, CancellationToken.None);
        response.Balance.Should().Be(initialBalance + 1);
    }
}