using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Devices;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Core.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Devices;

[Collection("Integration Tests Collection")]
public class RetireDeviceHandlerTests : BaseIntegrationTest
{
    private readonly RetireDeviceHandler _handler;
    private readonly IFusionCache _cache;
    
    public RetireDeviceHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _handler = new RetireDeviceHandler(DbContext, _cache);
    }

    [Fact]
    public async Task Should_RetireDevice_In_Database()
    {
        var device = await CreateDeviceAsync(status: DeviceStatus.Available);
        device.Status.Should().NotBe(DeviceStatus.Retired);
        
        await _handler.Handle(device.Id, CancellationToken.None);
        
        var deviceInDb = await DbContext.Devices.FindAsync(device.Id);
        
        deviceInDb.Should().NotBeNull();
        deviceInDb.Status.Should().Be(DeviceStatus.Retired);
    }

    [Fact]
    public async Task Should_Throw_DeviceNotFoundException_When_DeviceDoesNotExist()
    {
        await Assert.ThrowsAsync<DeviceNotFoundException>(() => _handler.Handle(1, CancellationToken.None));
    }

    [Fact]
    public async Task Should_Throw_DeviceIsBookedException_When_DeviceIsBooked()
    {
        var device = await CreateDeviceAsync(status: DeviceStatus.Rented);
        await Assert.ThrowsAsync<DeviceIsBookedException>(() => _handler.Handle(device.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Should_InvalidateCache_After_Retire()
    {
        var device = await CreateDeviceAsync();

        const string cacheKey = "key";
        await _cache.SetAsync(cacheKey, "test", tags: [CacheTags.Devices]);
        var cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeTrue();
        
        await _handler.Handle(device.Id, CancellationToken.None);
        
        cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeFalse();
    }
}