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
        services.AddScoped<GetAllDevicesQueryHandler>();
        services.AddScoped<GetDeviceByIdQueryHandler>();
        services.AddScoped<CreateDeviceCommandHandler>();
        services.AddScoped<UpdateDeviceCommandHandler>();
        services.AddScoped<RetireDeviceCommandHandler>();

        services.AddScoped<GetAllBookingsQueryHandler>();
        services.AddScoped<GetBookingByIdQueryHandler>();
        services.AddScoped<GetUserBookingsQueryHandler>();
        services.AddScoped<CreateBookingCommandHandler>();
        services.AddScoped<CompleteBookingCommandHandler>();

        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<RefreshTokenCommandHandler>();
        services.AddScoped<RevokeRefreshTokenCommandHandler>();

        services.AddScoped<GetAllUsersQueryHandler>();
        services.AddScoped<GetUserByIdQueryHandler>();
        services.AddScoped<GetUserBalanceQueryHandler>();
        services.AddScoped<DepositUserBalanceCommandHandler>();
        return services;
    }
}