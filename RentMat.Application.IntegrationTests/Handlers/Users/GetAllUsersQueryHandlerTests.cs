using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Queries.Users;
using RentMat.Core.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Users;

[Collection("Integration Tests Collection")]
public class GetAllUsersQueryHandlerTests : BaseIntegrationTest
{
    private const string Login1 = "Alice";
    private const string Login2 = "Bob";
    private const string Login3 = "Charlie";
    private readonly IFusionCache _cache;

    private readonly GetAllUsersQueryHandler _queryHandler;

    public GetAllUsersQueryHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _queryHandler = new GetAllUsersQueryHandler(DbContext, _cache,
            scope.ServiceProvider.GetRequiredService<ILogger<GetAllUsersQueryHandler>>());
    }

    [Fact]
    public async Task Should_Return_EmptyList_When_There_Is_NoUsers()
    {
        var result = await _queryHandler.Handle(new GetAllUsersQuery(), CancellationToken.None);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Return_PaginatedResult()
    {
        const int userAmount = 15;
        await CreateUsersAsync(userAmount);

        const int pageSize = 10;
        var result = await _queryHandler.Handle(new GetAllUsersQuery(1, pageSize),
            CancellationToken.None);
        result.Items.Should().HaveCount(10);

        result = await _queryHandler.Handle(new GetAllUsersQuery(2, pageSize),
            CancellationToken.None);
        result.Items.Should().HaveCount(5);
    }

    [Theory]
    [InlineData("li", 2)]
    [InlineData(Login1, 1)]
    [InlineData(Login2, 1)]
    [InlineData("Not found", 0)]
    public async Task Should_Return_FilteredResult_By_Search(string search, int expectedCount)
    {
        await CreateUserAsync(Login1);
        await CreateUserAsync(Login2);
        await CreateUserAsync(Login3);

        var result = await _queryHandler.Handle(new GetAllUsersQuery(Search: search),
            CancellationToken.None);
        result.Items.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task Should_Return_FilteredResult_By_Role()
    {
        const UserRole role1 = UserRole.User;
        const UserRole role2 = UserRole.Admin;

        await CreateUserAsync(role: role1);
        await CreateUserAsync(role: role2);

        var result = await _queryHandler.Handle(new GetAllUsersQuery(Role: role1),
            CancellationToken.None);
        result.Items.Should().HaveCount(1);
        result.Items.Select(u => u.Role).Should().BeEquivalentTo(role1.ToString());
    }

    [Fact]
    public async Task Should_Return_CachedUsers_If_CacheExists()
    {
        const int userAmount = 5;
        await CreateUsersAsync(userAmount);

        var result = await _queryHandler.Handle(new GetAllUsersQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(userAmount);

        await CreateUserAsync();

        result = await _queryHandler.Handle(new GetAllUsersQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(userAmount);
    }

    [Fact]
    public async Task Should_Return_UpdatedData_After_CacheInvalidation()
    {
        const int userAmount = 5;
        await CreateUsersAsync(userAmount);

        var result = await _queryHandler.Handle(new GetAllUsersQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(userAmount);

        await CreateUserAsync();
        await _cache.RemoveByTagAsync(CacheTags.Users);

        result = await _queryHandler.Handle(new GetAllUsersQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(userAmount + 1);
    }
}