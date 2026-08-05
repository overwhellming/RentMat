using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using RentMat.Infrastructure.Data;
using Respawn;
using Testcontainers.PostgreSql;

namespace RentMat.Application.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("rentmat_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private string _connectionString = null!;
    private Respawner _respawner = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();

        try
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HOST STARTUP FAILED: {ex}");
            throw;
        }

        await InitializeRespawnerAsync();
    }

    public new async Task DisposeAsync()
    {
        await _container.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "SuperSecretKey1234567890SuperSecretKey1234567890" },
                { "Jwt:Issuer", "RentMatIssuer" },
                { "Jwt:Audience", "RentMatAudience" }
            });
        });

        builder.ConfigureTestServices(services =>
        {
            var service = services
                .SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (service is not null)
                services.Remove(service);

            services.AddDbContext<AppDbContext>(options =>
            {
                options
                    .UseNpgsql(_container.GetConnectionString());
            });
        });

        builder.ConfigureLogging(logging =>
            logging.ClearProviders());
    }

    private async Task InitializeRespawnerAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }
}