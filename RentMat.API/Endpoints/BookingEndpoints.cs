using System.Security.Claims;
using RentMat.API.Common.Security;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Application.Handlers.Booking;
using RentMat.Application.Queries;

namespace RentMat.API.Endpoints;

internal static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapGet("/", GetAll)
            .RequireAuthorization(Policies.AdminOnly);
        
        group.MapGet("/{id:int}", GetById)
            .RequireAuthorization(Policies.AdminOnly);
        
        group.MapGet("me", GetMy)
            .RequireAuthorization();
        
        group.MapPost("/", Create).RequireAuthorization();
        
        group.MapPost("/{id:int}/complete", Complete)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetAll(GetAllBookingsQuery query, GetAllBookingsHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.Handle(query, cancellationToken));
    }

    private static async Task<IResult> GetById(int id, GetBookingByIdHandler handler, CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.Handle(id, cancellationToken));
    }

    private static async Task<IResult> GetMy(ClaimsPrincipal user, GetUserBookingsHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(handler.Handle(user.GetUserId(), cancellationToken));
    }

    private static async Task<IResult> Create(BookingCreateDto dto, ClaimsPrincipal user, CreateBookingHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.Handle(dto, user.GetUserId(), cancellationToken));
    }

    private static async Task<IResult> Complete(int id, ClaimsPrincipal user, CompleteBookingHandler handler, CancellationToken cancellationToken)
    {
        await handler.Handle(id, user.GetUserId(), cancellationToken);
        return Results.NoContent();
    }
}