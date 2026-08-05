using RentMat.Application.Handlers.Devices;

namespace RentMat.API.Registrars;

internal static class MediatrRegistrar
{
    public static IServiceCollection RegisterMediatr(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllDevicesQueryHandler).Assembly));
        return services;
    }
}