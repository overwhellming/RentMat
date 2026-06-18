using Microsoft.EntityFrameworkCore;
using RentMat.Infrastructure.Data;

namespace RentMat.API.Registrars;

internal static class DatabaseRegistrar
{
    public static IServiceCollection RegisterDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
        });

        return services;
    }
}