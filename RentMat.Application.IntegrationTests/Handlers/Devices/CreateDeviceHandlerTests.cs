using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Exceptions.Devices;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.IntegrationTests.Infrastructure;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Devices;

[Collection("Integration Tests Collection")]
public class CreateDeviceHandlerTests : BaseIntegrationTest
{
    private readonly CreateDeviceHandler _handler;
    private readonly IFusionCache _cache;
    
    public CreateDeviceHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _handler = new CreateDeviceHandler(DbContext, _cache);
    }

    [Fact]
    public async Task Should_CreateDevice_In_Database()
    {
        const decimal rentPrice = 100;
        const string deviceName = "Laptop";
        const string categoryName = "Device";
        
        var category = await CreateDeviceCategoryAsync(name: categoryName);
        var dto = new DeviceCreateDto(deviceName, rentPrice, category.Id);

        var response = await _handler.Handle(dto, CancellationToken.None);
        response.Name.Should().Be(dto.Name);
        response.CategoryName.Should().Be(categoryName);
        response.HourRentPrice.Should().Be(rentPrice);
        response.CategoryName.Should().Be(category.Name);
        
        var deviceInDb = await DbContext.Devices.FindAsync(response.Id);
        deviceInDb.Should().NotBeNull();
        deviceInDb.Name.Should().Be(deviceName);
        deviceInDb.HourRentPrice.Should().Be(rentPrice);
        deviceInDb.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task Should_Throw_DeviceCategoryNotFoundException_When_CategoryDoesNotExist()
    {
        const decimal rentPrice = 100;
        const string deviceName = "Laptop";
        const int notExistingId = 999;
        
        var dto = new DeviceCreateDto(deviceName, rentPrice, notExistingId);

        await Assert.ThrowsAsync<DeviceCategoryNotFoundException>(() => _handler.Handle(dto, CancellationToken.None));
    }

    [Fact]
    public async Task Should_InvalidateCache_After_Creation()
    {
        const decimal rentPrice = 100;
        const string deviceName = "Laptop";
        const string categoryName = "Device";
        
        var category = await CreateDeviceCategoryAsync(name: categoryName);
        var dto = new DeviceCreateDto(deviceName, rentPrice, category.Id);

        const string cacheKey = "key";
        await _cache.SetAsync(cacheKey, "test", tags: [CacheTags.Devices]);
        var cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeTrue();
        
        await _handler.Handle(dto, CancellationToken.None);
        
        cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeFalse();
    }
}