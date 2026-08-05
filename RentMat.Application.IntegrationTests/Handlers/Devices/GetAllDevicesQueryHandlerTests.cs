using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Queries.Devices;
using RentMat.Core.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Devices;

[Collection("Integration Tests Collection")]
public class GetAllDevicesQueryHandlerTests : BaseIntegrationTest
{
    private const string Name1 = "Laptop";
    private const string Name2 = "Phone";
    private const string Name3 = "Console";
    private readonly IFusionCache _cache;

    private readonly GetAllDevicesQueryHandler _queryHandler;

    public GetAllDevicesQueryHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _queryHandler = new GetAllDevicesQueryHandler(DbContext, _cache,
            scope.ServiceProvider.GetRequiredService<ILogger<GetAllDevicesQueryHandler>>());
    }

    [Fact]
    public async Task Should_Return_EmptyList_When_There_Is_NoDevices()
    {
        var result = await _queryHandler.Handle(new GetAllDevicesQuery(), CancellationToken.None);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Return_PaginatedResult()
    {
        const int deviceAmount = 15;
        await CreateDevicesAsync(deviceAmount);

        const int pageSize = 10;
        var result = await _queryHandler.Handle(new GetAllDevicesQuery(1, pageSize),
            CancellationToken.None);
        result.Items.Should().HaveCount(10);

        result = await _queryHandler.Handle(new GetAllDevicesQuery(2, pageSize),
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
        await CreateDeviceAsync(Name1);
        await CreateDeviceAsync(Name2);
        await CreateDeviceAsync(Name3);

        var result = await _queryHandler.Handle(new GetAllDevicesQuery(Search: search),
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

        var result = await _queryHandler.Handle(new GetAllDevicesQuery(Status: status1),
            CancellationToken.None);
        result.Items.Should().HaveCount(1);
        result.Items.Select(u => u.StatusName).Should().BeEquivalentTo(status1.ToString());
    }

    [Fact]
    public async Task Should_Return_CachedUsers_If_CacheExists()
    {
        const int deviceAmount = 5;
        await CreateDevicesAsync(deviceAmount);

        var result = await _queryHandler.Handle(new GetAllDevicesQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(deviceAmount);

        await CreateUserAsync();

        result = await _queryHandler.Handle(new GetAllDevicesQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(deviceAmount);
    }

    [Fact]
    public async Task Should_Return_UpdatedData_After_CacheInvalidation()
    {
        const int deviceAmount = 5;
        await CreateDevicesAsync(deviceAmount);

        var result = await _queryHandler.Handle(new GetAllDevicesQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(deviceAmount);

        await CreateDeviceAsync();
        await _cache.RemoveByTagAsync(CacheTags.Devices);

        result = await _queryHandler.Handle(new GetAllDevicesQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(deviceAmount + 1);
    }
}