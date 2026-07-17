using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.Handlers.Users;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Queries;
using RentMat.Core.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Devices;

[CollectionDefinition("Integration Test Collection")]
public class GetAllDevicesHandlerTests : BaseIntegrationTest
{
    private const string Name1 = "Laptop";
    private const string Name2 = "Phone";
    private const string Name3 = "Console";
    
    private readonly GetAllDevicesHandler _handler;
    private readonly IFusionCache _cache;
    
    public GetAllDevicesHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _handler = new GetAllDevicesHandler(DbContext, _cache,
            scope.ServiceProvider.GetRequiredService<ILogger<GetAllDevicesHandler>>());
    }

    [Fact]
    public async Task Should_Return_EmptyList_When_There_Is_NoDevices()
    {
        var result = await _handler.Handle(new GetAllDevicesQuery(), CancellationToken.None);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Return_PaginatedResult()
    {
        const int deviceAmount = 15;
        await CreateDevicesAsync(amount: deviceAmount);

        const int pageSize = 10;
        var result = await _handler.Handle(new GetAllDevicesQuery(Page: 1, PageSize: pageSize),
            CancellationToken.None);
        result.Items.Should().HaveCount(10);

        result = await _handler.Handle(new GetAllDevicesQuery(Page: 2, PageSize: pageSize),
            CancellationToken.None);
        result.Items.Should().HaveCount(5);
    }

    [Theory]
    [InlineData("la", 1)]
    [InlineData(Name1, 1)]
    [InlineData(Name2, 1)]
    [InlineData("Not found", 0)]
    public async Task Should_Return_FilteredResult_By_Search(string search, int expectedCount)
    {
        await CreateDeviceAsync(name: Name1);
        await CreateDeviceAsync(name: Name2);
        await CreateDeviceAsync(name: Name3);

        var result = await _handler.Handle(new GetAllDevicesQuery(Search: search),
            CancellationToken.None);
        result.Items.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task Should_Return_FilteredResult_By_Status()
    {
        const DeviceStatus status1 = DeviceStatus.Available;
        const DeviceStatus status2 = DeviceStatus.Maintenance;

        await CreateDeviceAsync(status: status1);
        await CreateDeviceAsync(status: status2);

        var result = await _handler.Handle(new GetAllDevicesQuery(Status: status1), 
            CancellationToken.None);
        result.Items.Should().HaveCount(1);
        result.Items.Select(u => u.StatusName).Should().BeEquivalentTo(status1.ToString());
    }

    [Theory]
    [InlineData(GetAllDevicesHandler.MaxPageSize + 1, GetAllDevicesHandler.MaxPageSize)]
    [InlineData(0, GetAllDevicesHandler.DefaultPageSize)]
    [InlineData(25, 25)]
    public async Task Should_Apply_CorrectPageSize(int inputPageSize, int expectedPageSize)
    {
        var result = await _handler.Handle(new GetAllDevicesQuery(PageSize: inputPageSize), 
            CancellationToken.None);
        result.PageSize.Should().Be(expectedPageSize);
    }
    
    [Fact]
    public async Task Should_Constraint_Min_Page()
    {
        var result = await _handler.Handle(new GetAllDevicesQuery(Page: 0 ),
            CancellationToken.None);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task Should_Return_CachedUsers_When_DataChanged()
    {
        const int deviceAmount = 5;
        await CreateDevicesAsync(amount: deviceAmount);

        var result = await _handler.Handle(new GetAllDevicesQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(deviceAmount);

        await CreateUserAsync();
        
        result = await _handler.Handle(new GetAllDevicesQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(deviceAmount);
    }
    
    [Fact]
    public async Task Should_Return_UpdatedData_After_CacheInvalidation()
    {
        const int deviceAmount = 5;
        await CreateDevicesAsync(amount: deviceAmount);

        var result = await _handler.Handle(new GetAllDevicesQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(deviceAmount);

        await CreateDeviceAsync();
        await _cache.RemoveByTagAsync(CacheTags.Devices);
        
        result = await _handler.Handle(new GetAllDevicesQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(deviceAmount + 1);
    }
}