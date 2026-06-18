using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Handlers.Authentication;
using RentMat.Application.Handlers.Booking;
using RentMat.Application.Handlers.Devices;

namespace RentMat.Application.Registrars;

public static class HandlerRegistrar
{
    public static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        services.AddScoped<GetDevicesHandler>();
        services.AddScoped<BookDeviceHandler>();
        
        services.AddScoped<CompleteBookingHandler>();
        services.AddScoped<GetBookingsHandler>();
        
        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        return services;
    }
}