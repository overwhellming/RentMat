using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Exceptions.Devices;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Core.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Devices;

[Collection("Integration Tests Collection")]
public class UpdateDeviceHandlerTests : BaseIntegrationTest
{
    private readonly UpdateDeviceHandler _handler;
    private readonly IFusionCache _cache;
    public UpdateDeviceHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _handler = new UpdateDeviceHandler(DbContext, _cache);
    }

    [Fact]
    public async Task Should_UpdateDevice_In_Database()
    {
        var category = await CreateDeviceCategoryAsync();
        
        const string newName = "Laptop";
        const decimal newRentPrice = 100;
        var newCategoryId = category.Id;
        
        var device = await CreateDeviceAsync();
        device.Name.Should().NotBe(newName);
        device.HourRentPrice.Should().NotBe(newRentPrice);
        device.CategoryId.Should().NotBe(newCategoryId);
        
        var dto = new DeviceUpdateDto(newName, newRentPrice, newCategoryId);
        await _handler.Handle(device.Id, dto, CancellationToken.None);

        var deviceInDb = await DbContext.Devices.FindAsync(device.Id);
        deviceInDb!.Name.Should().Be(newName);
        deviceInDb.HourRentPrice.Should().Be(newRentPrice);
        deviceInDb.CategoryId.Should().Be(newCategoryId);
    }

    [Fact]
    public async Task Should_Throw_DeviceCategoryNotFoundException_When_CategoryDoesNotExist()
    {
        const string newName = "Laptop";
        const decimal newRentPrice = 100;
        const int notExistingId = 999;
        
        var device = await CreateDeviceAsync();
        var dto = new DeviceUpdateDto(newName, newRentPrice, notExistingId);

        await Assert.ThrowsAsync<DeviceCategoryNotFoundException>(() =>
            _handler.Handle(device.Id, dto, CancellationToken.None));
    }
    [Fact]
    public async Task Should_Throw_DeviceNotFoundException_WhenDeviceDoesNotExist()
    {
        var category = await CreateDeviceCategoryAsync();
        
        const string newName = "Laptop";
        const decimal newRentPrice = 100;
        var newCategoryId = category.Id;
        
        var dto = new DeviceUpdateDto(newName, newRentPrice, newCategoryId);

        const int notExistingId = 999;
        await Assert.ThrowsAsync<DeviceNotFoundException>(() =>
            _handler.Handle(notExistingId, dto, CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_Throw_DeviceIsBookedException_WhenDeviceIsBooked()
    {
        var category = await CreateDeviceCategoryAsync();
        
        const string newName = "Laptop";
        const decimal newRentPrice = 100;
        var newCategoryId = category.Id;
        
        var device = await CreateDeviceAsync(status:DeviceStatus.Rented);
        var dto = new DeviceUpdateDto(newName, newRentPrice, newCategoryId);

        await Assert.ThrowsAsync<DeviceIsBookedException>(() =>
            _handler.Handle(device.Id, dto, CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_InvalidateCache_After_Retire()
    {
        var category = await CreateDeviceCategoryAsync();
        
        const string newName = "Laptop";
        const decimal newRentPrice = 100;
        var newCategoryId = category.Id;
        
        var device = await CreateDeviceAsync();
        var dto = new DeviceUpdateDto(newName, newRentPrice, newCategoryId);

        const string cacheKey = "key";
        await _cache.SetAsync(cacheKey, "test", tags: [CacheTags.Devices]);
        var cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeTrue();
        
        await _handler.Handle(device.Id, dto, CancellationToken.None);
        
        cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeFalse();
    }
}