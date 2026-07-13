using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.DTOs.Authentication;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;

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

    protected async Task<User> CreateUserAsync(RegisterDto dto)
    {
        var user = new User
            { Login = dto.Login, Email = dto.Email, HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password) };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();
        return user;
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