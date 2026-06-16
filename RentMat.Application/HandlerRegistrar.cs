using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Booking;
using RentMat.Application.Devices;

namespace RentMat.Application;

public static class HandlerRegistrar
{
    public static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        services.AddScoped<GetDevicesHandler>();
        services.AddScoped<BookDeviceHandler>();
        return services;
    }
}