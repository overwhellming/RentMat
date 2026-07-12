using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403);

        group.MapGet("/{id:int}", GetById)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("GetUserById")
            .WithSummary("Returns a user by id")
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);

        group.MapGet("/me", GetMe)
            .RequireAuthorization()
            .WithName("GetMe")
            .WithSummary("Returns the current user")
            .ProducesProblem(401);

        group.MapGet("/me/balance", GetMyBalance)
            .RequireAuthorization()
            .WithName("GetMyBalance")
            .WithSummary("Returns the current user's balance")
            .ProducesProblem(401);

        group.MapPost("/me/balance", Deposit)
            .RequireAuthorization()
            .WithName("Deposit")
            .WithSummary("Deposit to the current user's balance")
            .ProducesValidationProblem()
            .ProducesProblem(400)
            .ProducesProblem(401);
        
        group.MapGet("/me/deposits", GetMyDeposits)
            .RequireAuthorization()
            .WithName("GetMyDeposits")
            .WithSummary("Returns the current user's deposits")
            .ProducesProblem(401);
    }

    private static async Task<Ok<PagedResponse<UserResponseDto>>> GetAll([AsParameters] GetAllUsersQuery query,
        [FromServices] GetAllUsersHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(query, cancellationToken));
    }

    private static async Task<Ok<UserResponseDto>> GetById(int id, [FromServices] GetUserByIdHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(id, cancellationToken));
    }

    private static async Task<Ok<UserResponseDto>> GetMe(ClaimsPrincipal user,[FromServices]  GetUserByIdHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(user.GetUserId(), cancellationToken));
    }

    private static async Task<Ok<decimal>> GetMyBalance(ClaimsPrincipal user, [FromServices] GetUserBalanceHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(user.GetUserId(), cancellationToken));
    }

    private static async Task<Ok<DepositCreatedResponseDto>> Deposit(DepositCreateDto dto, ClaimsPrincipal user,
        [FromServices]   DepositUserBalanceHandler handler, CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(dto.Amount, user.GetUserId(), cancellationToken));
    }

    private static async Task<Ok<IEnumerable<DepositResponseDto>>> GetMyDeposits(ClaimsPrincipal user,
        [FromServices]  GetUserDepositsHandler handler, CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(user.GetUserId(), cancellationToken));
    }
}