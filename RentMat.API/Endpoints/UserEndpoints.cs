using System.Security.Claims;
using RentMat.API.Common.Security;
using RentMat.Application.Common;
using RentMat.Application.DTOs.User;
using RentMat.Application.Handlers.Users;
using RentMat.Application.Queries;

namespace RentMat.API.Endpoints;

internal static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("/", GetAll)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("GetUsers")
            .WithSummary("Returns all users")
            .Produces<PagedResponse<UserResponseDto>>()
            .Produces(400)
            .Produces(401)
            .Produces(403);

        group.MapGet("/{id:int}", GetById)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("GetUserById")
            .WithSummary("Returns a user by id")
            .Produces<UserResponseDto>()
            .Produces(400)
            .Produces(401)
            .Produces(403);

        group.MapGet("/me", GetMe)
            .RequireAuthorization()
            .WithName("GetMe")
            .WithSummary("Returns the current user")
            .Produces<UserResponseDto>()
            .Produces(401);

        group.MapGet("/me/balance", GetMyBalance)
            .RequireAuthorization()
            .WithName("GetMyBalance")
            .WithSummary("Returns the current user's balance")
            .Produces<decimal>()
            .Produces(401);

        group.MapPost("/me/balance", Deposit)
            .RequireAuthorization()
            .WithName("Deposit")
            .WithSummary("Deposit to the current user's balance")
            .Produces<DepositResponseDto>()
            .Produces(401);
    }

    private static async Task<IResult> GetAll(GetAllUsersQuery query, GetAllUsersHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.Handle(query, cancellationToken));
    }

    private static async Task<IResult> GetById(int id, GetUserByIdHandler handler, CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.Handle(id, cancellationToken));
    }

    private static async Task<IResult> GetMe(ClaimsPrincipal user, GetUserByIdHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.Handle(user.GetUserId(), cancellationToken));
    }

    private static async Task<IResult> GetMyBalance(ClaimsPrincipal user, GetUserBalanceHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.Handle(user.GetUserId(), cancellationToken));
    }

    private static async Task<IResult> Deposit(DepositCreateDto dto, ClaimsPrincipal user,
        DepositUserBalanceHandler handler, CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.Handle(dto.Amount, user.GetUserId(), cancellationToken));
    }
}