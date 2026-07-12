using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RentMat.API.Common.Security;
using RentMat.Application.Common;
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
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("GetBookings")
            .WithSummary("Returns all bookings")
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403);
        
        group.MapGet("/{id:int}", GetById)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("GetBookingById")
            .WithSummary("Returns a booking by id")
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);

        group.MapGet("/me", GetMy)
            .RequireAuthorization()
            .WithName("GetMyBookings")
            .WithSummary("Returns the current user's bookings")
            .ProducesProblem(401);

        group.MapPost("/", Create)
            .RequireAuthorization()
            .WithName("CreateBooking")
            .WithSummary("Creates a booking")
            .ProducesValidationProblem()
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(409);
        
        group.MapPost("/{id:int}/complete", Complete)
            .RequireAuthorization()
            .WithName("CompleteBooking")
            .WithSummary("Completes a booking")
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);
    }

    private static async Task<Ok<PagedResponse<BookingResponseDto>>> GetAll([AsParameters] GetAllBookingsQuery query, [FromServices] GetAllBookingsHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(query, cancellationToken));
    }

    private static async Task<Ok<BookingResponseDto>> GetById(int id,[FromServices]  GetBookingByIdHandler handler, CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(id, cancellationToken));
    }

    private static async Task<Ok<IEnumerable<BookingResponseDto>>> GetMy(ClaimsPrincipal user, [FromServices] GetUserBookingsHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(user.GetUserId(), cancellationToken));
    }

    private static async Task<CreatedAtRoute<BookingResponseDto>> Create(BookingCreateDto dto, ClaimsPrincipal user, [FromServices]  CreateBookingHandler handler,
        CancellationToken cancellationToken)
    {
        var booking = await handler.Handle(dto, user.GetUserId(), cancellationToken);
        return TypedResults.CreatedAtRoute(booking, routeName: "GetBookingById", routeValues: new {id = booking.Id});
    }

    private static async Task<NoContent> Complete(int id, ClaimsPrincipal user, [FromServices] CompleteBookingHandler handler, CancellationToken cancellationToken)
    {
        await handler.Handle(id, user.GetUserId(), cancellationToken);
        return TypedResults.NoContent();
    }
}