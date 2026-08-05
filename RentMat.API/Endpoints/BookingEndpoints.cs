using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RentMat.API.Common.Security;
using RentMat.Application.Commands.Booking;
using RentMat.Application.Common;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Application.Handlers.Booking;
using RentMat.Application.Queries.Booking;
using RentMat.Application.Queries.Users;

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
            .ProducesValidationProblem()
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
            .ProducesProblem(401)
            .ProducesProblem(404)
            .ProducesProblem(409);
        
        group.MapPost("/me/{id:int}/complete", Complete)
            .RequireAuthorization()
            .WithName("CompleteCurrentUsersBooking")
            .WithSummary("Completes a current user's booking")
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);
    }

    private static async Task<Ok<PagedResponse<BookingResponseDto>>> GetAll(
        [AsParameters] GetAllBookingsQuery query, 
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await mediator.Send(query, cancellationToken));
    }

    private static async Task<Ok<BookingResponseDto>> GetById(
        [AsParameters] int id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await mediator.Send(new GetBookingByIdQuery(id), cancellationToken));
    }

    private static async Task<CreatedAtRoute<BookingResponseDto>> Create(
        [FromBody] BookingCreateDto dto, 
        ClaimsPrincipal user, 
        [FromServices]  IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateBookingCommand(dto.DeviceId, dto.UserId, dto.StartDate, dto.EndDate);
        var booking = await mediator.Send(command, cancellationToken);
        return TypedResults.CreatedAtRoute(booking, routeName: "GetBookingById", routeValues: new {id = booking.Id});
    }

    private static async Task<Ok<IEnumerable<BookingResponseDto>>> GetMy(
        ClaimsPrincipal user, 
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await mediator.Send(new GetUserBookingsQuery(user.GetUserId()), cancellationToken));
    }

    private static async Task<NoContent> Complete(
        int id, 
        ClaimsPrincipal user, 
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new CompleteBookingCommand(id, user.GetUserId()), cancellationToken);
        return TypedResults.NoContent();
    }
}