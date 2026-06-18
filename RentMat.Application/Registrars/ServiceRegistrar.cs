using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Services;
using RentMat.Application.Services.Interfaces;

namespace RentMat.Application.Registrars;

public static class ServiceRegistrar
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}