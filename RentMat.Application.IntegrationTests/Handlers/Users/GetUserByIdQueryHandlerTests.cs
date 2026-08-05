using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Queries.Users;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Users;

[Collection("Integration Tests Collection")]
public class GetUserByIdQueryHandlerTests : BaseIntegrationTest
{
    private readonly GetUserByIdQueryHandler _queryHandler;
    private readonly IFusionCache _cache;
    
    public GetUserByIdQueryHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _queryHandler = new GetUserByIdQueryHandler(DbContext, _cache,
            scope.ServiceProvider.GetRequiredService<ILogger<GetUserByIdQueryHandler>>());
    }

    [Fact]
    public async Task Should_Return_User()
    {
        const string login = "Alexi";
        var user = await CreateUserAsync(login: login);
        var response = await _queryHandler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        response.Should().NotBeNull();
        response.Id.Should().Be(user.Id);
        response.Login.Should().Be(login);
    }

    [Fact]
    public async Task Should_Throw_UserNotFoundException_When_UserDoesNotExist()
    {
        const int notExistingId = 999;
        await Assert.ThrowsAsync<UserNotFoundException>(() => _queryHandler.Handle(new GetUserByIdQuery(notExistingId), CancellationToken.None));
    }

    [Fact]
    public async Task Should_Return_CachedUser_If_CacheExists()
    {
        const decimal initialBalance = 100;
        var user = await CreateUserAsync(balance: initialBalance);
        
        var response = await _queryHandler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);
        response.Balance.Should().Be(initialBalance);

        var userInDb = await DbContext.Users.FindAsync(user.Id);
        userInDb!.Balance = initialBalance + 1;
        await DbContext.SaveChangesAsync();
        
        response = await _queryHandler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);
        response.Balance.Should().Be(initialBalance);
    }
    
    [Fact]
    public async Task Should_Return_UpdatedData_After_CacheInvalidation()
    {
        const decimal initialBalance = 100;
        var user = await CreateUserAsync(balance: initialBalance);
        
        var response = await _queryHandler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);
        response.Balance.Should().Be(initialBalance);

        var userInDb = await DbContext.Users.FindAsync(user.Id);
        userInDb!.Balance = initialBalance + 1;
        await DbContext.SaveChangesAsync();
        await _cache.RemoveByTagAsync(CacheTags.Users);
        
        response = await _queryHandler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);
        response.Balance.Should().Be(initialBalance + 1);
    }
}