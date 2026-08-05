using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Devices;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Queries.Devices;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Devices;

[Collection("Integration Tests Collection")]
public class GetDeviceByIdQueryHandlerTests : BaseIntegrationTest
{
    private readonly IFusionCache _cache;
    private readonly GetDeviceByIdQueryHandler _queryHandler;

    public GetDeviceByIdQueryHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _queryHandler = new GetDeviceByIdQueryHandler(DbContext, _cache,
            scope.ServiceProvider.GetRequiredService<ILogger<GetDeviceByIdQueryHandler>>());
    }

    [Fact]
    public async Task Should_Return_Device()
    {
        const string name = "Laptop";
        var device = await CreateDeviceAsync(name);
        var response = await _queryHandler.Handle(new GetDeviceByIdQuery(device.Id), CancellationToken.None);

        response.Should().NotBeNull();
        response.Id.Should().Be(device.Id);
        response.Name.Should().Be(name);
    }

    [Fact]
    public async Task Should_Throw_DeviceNotFoundException_When_DeviceDoesNotExist()
    {
        const int notExistingId = 999;
        await Assert.ThrowsAsync<DeviceNotFoundException>(() =>
            _queryHandler.Handle(new GetDeviceByIdQuery(notExistingId), CancellationToken.None));
    }

    [Fact]
    public async Task Should_Return_CachedDevice_If_CacheExists()
    {
        const string initialName = "Laptop";
        var device = await CreateDeviceAsync(initialName);

        var response = await _queryHandler.Handle(new GetDeviceByIdQuery(device.Id), CancellationToken.None);
        response.Name.Should().Be(initialName);

        var userInDb = await DbContext.Devices.FindAsync(device.Id);
        userInDb!.Name = initialName + "a";
        await DbContext.SaveChangesAsync();

        response = await _queryHandler.Handle(new GetDeviceByIdQuery(device.Id), CancellationToken.None);
        response.Name.Should().Be(initialName);
    }

    [Fact]
    public async Task Should_Return_UpdatedData_After_CacheInvalidation()
    {
        const string initialName = "Laptop";
        var device = await CreateDeviceAsync(initialName);

        var response = await _queryHandler.Handle(new GetDeviceByIdQuery(device.Id), CancellationToken.None);
        response.Name.Should().Be(initialName);

        var deviceInDb = await DbContext.Devices.FindAsync(device.Id);

        const string newName = initialName + "a";
        deviceInDb!.Name = newName;
        await DbContext.SaveChangesAsync();
        await _cache.RemoveByTagAsync(CacheTags.Devices);

        response = await _queryHandler.Handle(new GetDeviceByIdQuery(device.Id), CancellationToken.None);
        response.Name.Should().Be(newName);
    }
}