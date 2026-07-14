using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.DTOs.Authentication;
using RentMat.Core.Enums;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;
using Serilog;

namespace RentMat.Application.IntegrationTests.Infrastructure;

[Collection("Integration Tests Collection")]
public class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly IServiceScope _scope;
    protected readonly AppDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    protected async Task<User> CreateUserAsync(string? login = null, string? email = null, UserRole role = UserRole.User, 
        string password = "Password123")
    {
        var suffix = Guid.NewGuid().ToString()[..6];
        var user = new User
            { Login = login ?? $"John_{suffix}", Email = email ?? $"john{suffix}@test.com", Role = role,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(password) };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();
        return user;
    }

    protected async Task<List<User>> CreateUsersAsync(int amount = 10)
    {
        const string password = "Password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
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

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}