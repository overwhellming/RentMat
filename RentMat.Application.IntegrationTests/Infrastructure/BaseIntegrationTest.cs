using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Common;
using RentMat.Application.DTOs.Authentication;
using RentMat.Core.Enums;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Infrastructure;

[Collection("Integration Tests Collection")]
public class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly IServiceScope _scope;
    private readonly IFusionCache _cache;
    protected readonly AppDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _cache = _scope.ServiceProvider.GetRequiredService<IFusionCache>();
    }

    protected async Task<User> CreateUserAsync(string? login = null, string? email = null, UserRole role = UserRole.User, 
        string password = "Password123", decimal balance = 0)
    {
        var suffix = Guid.NewGuid().ToString()[..6];
        var user = new User
            { Login = login ?? $"John_{suffix}", Email = email ?? $"john{suffix}@test.com", Role = role,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor:4), Balance = balance};
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();
        return user;
    }

    protected async Task<DeviceCategory> CreateDeviceCategoryAsync(string? name = null)
    {
        var suffix = Guid.NewGuid().ToString()[..6];
        var category = new DeviceCategory
        {
            Name = name ?? $"Category_{suffix}"
        };
        DbContext.DeviceCategories.Add(category);
        await DbContext.SaveChangesAsync();
        return category;
    }

    protected async Task<Device> CreateDeviceAsync(string? name = null, decimal hourRentPrice = 0, 
        DeviceStatus status = DeviceStatus.Available, string? categoryName = null)
    {
        var suffix = Guid.NewGuid().ToString()[..6];
        var category = new DeviceCategory
        {
            Name = categoryName ?? $"Category_{suffix}"
        };
        DbContext.DeviceCategories.Add(category);
        await DbContext.SaveChangesAsync();

        var device = new Device
        {
            Name = name ?? $"Device{suffix}",
            HourRentPrice = hourRentPrice,
            Status = status,
            CategoryId = category.Id
        };
        DbContext.Devices.Add(device);
        await DbContext.SaveChangesAsync();
        
        return device;
    }
    
    protected async Task<List<User>> CreateUsersAsync(int amount = 10)
    {
        const string password = "Password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor:4);
        var users = new List<User>();

        for (var i = 0; i < amount; i++)
        {
            var user = new User
            {
                Login = $"User_{i}",
                Email = $"user{i}@test.com",
                HashedPassword = hashedPassword
            };
            users.Add(user);
        }
        
        DbContext.AddRange(users);
        await DbContext.SaveChangesAsync();
        return users;
    }
    
    protected async Task<List<Device>> CreateDevicesAsync(int amount = 10)
    {
        var devices = new List<Device>();

        var category = await CreateDeviceCategoryAsync();
        
        for (var i = 0; i < amount; i++)
        {
            var user = new Device
            {
                Name = $"Device_{i}",
                HourRentPrice = 0,
                CategoryId = category.Id,
                Status = DeviceStatus.Available
            };
            devices.Add(user);
        }
        
        DbContext.AddRange(devices);
        await DbContext.SaveChangesAsync();
        return devices;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await _cache.ClearAsync();
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}