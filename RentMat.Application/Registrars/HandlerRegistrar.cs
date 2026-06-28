using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Handlers.Authentication;
using RentMat.Application.Handlers.Booking;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.Handlers.Users;

namespace RentMat.Application.Registrars;

public static class HandlerRegistrar
{
    public static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        services.AddScoped<GetAllDevicesHandler>();
        services.AddScoped<GetDeviceByIdHandler>();
        services.AddScoped<CreateDeviceHandler>();
        services.AddScoped<UpdateDeviceHandler>();
        services.AddScoped<RetireDeviceHandler>();

        services.AddScoped<GetAllBookingsHandler>();
        services.AddScoped<GetBookingByIdHandler>();
        services.AddScoped<GetUserBookingsHandler>();
        services.AddScoped<CreateBookingHandler>();
        services.AddScoped<CompleteBookingHandler>();

        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();

        services.AddScoped<GetAllUsersHandler>();
        services.AddScoped<GetUserByIdHandler>();
        services.AddScoped<GetUserBalanceHandler>();
        services.AddScoped<DepositUserBalanceHandler>();
        return services;
    }
}