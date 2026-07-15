using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Queries;
using RentMat.Core.Enums;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Users;

[CollectionDefinition("Integration Test Collection")]
public class GetAllUsersHandlerTests : BaseIntegrationTest
{
    private const string Login1 = "Alice";
    private const string Login2 = "Bob";
    private const string Login3 = "Charlie";
    
    private readonly GetAllUsersHandler _handler;
    private readonly IFusionCache _cache;

    public GetAllUsersHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _handler = new GetAllUsersHandler(DbContext, _cache,
            scope.ServiceProvider.GetRequiredService<ILogger<GetAllUsersHandler>>());
    }

    [Fact]
    public async Task Should_Return_EmptyList_When_There_Is_NoUsers()
    {
        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Return_PaginatedResult()
    {
        const int userAmount = 15;
        await CreateUsersAsync(amount: userAmount);

        const int pageSize = 10;
        var result = await _handler.Handle(new GetAllUsersQuery(Page: 1, PageSize: pageSize),
            CancellationToken.None);
        result.Items.Should().HaveCount(10);

        result = await _handler.Handle(new GetAllUsersQuery(Page: 2, PageSize: pageSize),
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
        await CreateUserAsync(login: Login1);
        await CreateUserAsync(login: Login2);
        await CreateUserAsync(login: Login3);

        var result = await _handler.Handle(new GetAllUsersQuery(Search: "li"),
            CancellationToken.None);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_Return_FilteredResult_By_Role()
    {
        const UserRole role1 = UserRole.User;
        const UserRole role2 = UserRole.Admin;

        await CreateUserAsync(role: role1);
        await CreateUserAsync(role: role2);

        var result = await _handler.Handle(new GetAllUsersQuery(Role: role1), 
            CancellationToken.None);
        result.Items.Should().HaveCount(1);
        result.Items.Select(u => u.Role).Should().BeEquivalentTo(role1.ToString());
    }

    [Theory]
    [InlineData(GetAllUsersHandler.MaxPageSize + 1, GetAllUsersHandler.MaxPageSize)]
    [InlineData(0, GetAllUsersHandler.DefaultPageSize)]
    [InlineData(25, 25)]
    public async Task Should_Apply_CorrectPageSize(int inputPageSize, int expectedPageSize)
    {
        var result = await _handler.Handle(new GetAllUsersQuery(PageSize: inputPageSize), 
            CancellationToken.None);
        result.PageSize.Should().Be(expectedPageSize);
    }
    
    [Fact]
    public async Task Should_Constraint_Min_Page()
    {
        var result = await _handler.Handle(new GetAllUsersQuery(Page: 0 ),
            CancellationToken.None);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task Should_Return_CachedUsers_When_DataChanged()
    {
        const int userAmount = 5;
        await CreateUsersAsync(amount: userAmount);

        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(userAmount);

        await CreateUserAsync();
        
        result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(userAmount);
    }
    
    [Fact]
    public async Task Should_Return_UpdatedData_After_CacheInvalidation()
    {
        const int userAmount = 5;
        await CreateUsersAsync(amount: userAmount);

        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(userAmount);

        await CreateUserAsync();
        await _cache.RemoveByTagAsync(CacheTags.Users);
        
        result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(userAmount + 1);
    }
}