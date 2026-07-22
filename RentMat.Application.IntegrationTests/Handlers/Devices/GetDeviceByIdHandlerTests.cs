using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Devices;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.IntegrationTests.Infrastructure;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Devices;

[Collection("Integration Tests Collection")]
public class GetDeviceByIdHandlerTests : BaseIntegrationTest
{
    private readonly GetDeviceByIdHandler _handler;
    private readonly IFusionCache _cache;
    
    public GetDeviceByIdHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _handler = new GetDeviceByIdHandler(DbContext, _cache,
            scope.ServiceProvider.GetRequiredService<ILogger<GetDeviceByIdHandler>>());
    }

    [Fact]
    public async Task Should_Return_Device()
    {
        const string name = "Laptop";
        var device = await CreateDeviceAsync(name: name);
        var response = await _handler.Handle(device.Id, CancellationToken.None);

        response.Should().NotBeNull();
        response.Id.Should().Be(device.Id);
        response.Name.Should().Be(name);
    }

    [Fact]
    public async Task Should_Throw_DeviceNotFoundException_When_DeviceDoesNotExist()
    {
        const int notExistingId = 999;
        await Assert.ThrowsAsync<DeviceNotFoundException>(() => _handler.Handle(notExistingId, CancellationToken.None));
    }

    [Fact]
    public async Task Should_Return_CachedDevice_If_CacheExists()
    {
        const string initialName = "Laptop";
        var device = await CreateDeviceAsync(name: initialName);
        
        var response = await _handler.Handle(device.Id, CancellationToken.None);
        response.Name.Should().Be(initialName);

        var userInDb = await DbContext.Devices.FindAsync(device.Id);
        userInDb!.Name = initialName + "a";
        await DbContext.SaveChangesAsync();
        
        response = await _handler.Handle(device.Id, CancellationToken.None);
        response.Name.Should().Be(initialName);
    }
    
    [Fact]
    public async Task Should_Return_UpdatedData_After_CacheInvalidation()
    {
        const string initialName = "Laptop";
        var user = await CreateDeviceAsync(name: initialName);
        
        var response = await _handler.Handle(user.Id, CancellationToken.None);
        response.Name.Should().Be(initialName);

        var userInDb = await DbContext.Devices.FindAsync(user.Id);

        const string newName = initialName + "a";
        userInDb!.Name = newName;
        await DbContext.SaveChangesAsync();
        await _cache.RemoveByTagAsync(CacheTags.Devices);
        
        response = await _handler.Handle(user.Id, CancellationToken.None);
        response.Name.Should().Be(newName);
    }
}