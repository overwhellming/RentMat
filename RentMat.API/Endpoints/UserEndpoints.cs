using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RentMat.API.Common.Security;
using RentMat.Application.Commands.Users;
using RentMat.Application.Common;
using RentMat.Application.DTOs.User;
using RentMat.Application.Handlers.Users;
using RentMat.Application.Queries.Users;

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
            .ProducesValidationProblem()
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
            .ProducesProblem(401);
        
        group.MapGet("/me/deposits", GetMyDeposits)
            .RequireAuthorization()
            .WithName("GetMyDeposits")
            .WithSummary("Returns the current user's deposits")
            .ProducesProblem(401);
    }

    private static async Task<Ok<PagedResponse<UserResponseDto>>> GetAll(
        [AsParameters] GetAllUsersQuery query,
        [FromServices] GetAllUsersQueryHandler queryHandler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await queryHandler.Handle(query, cancellationToken));
    }

    private static async Task<Ok<UserResponseDto>> GetById(
        [AsParameters] int id, 
        [FromServices] GetUserByIdQueryHandler queryHandler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await queryHandler.Handle(new GetUserByIdQuery(id), cancellationToken));
    }

    private static async Task<Ok<UserResponseDto>> GetMe(
        ClaimsPrincipal user,
        [FromServices]  GetUserByIdQueryHandler queryHandler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await queryHandler.Handle(new GetUserByIdQuery(user.GetUserId()), cancellationToken));
    }

    private static async Task<Ok<decimal>> GetMyBalance(
        ClaimsPrincipal user, 
        [FromServices] GetUserBalanceQueryHandler queryHandler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await queryHandler.Handle(new GetUserBalanceQuery(user.GetUserId()), cancellationToken));
    }

    private static async Task<Ok<DepositCreatedResponseDto>> Deposit(
        [FromBody] DepositCreateDto dto, 
        ClaimsPrincipal user,
        [FromServices] DepositUserBalanceCommandHandler commandHandler, 
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await commandHandler.Handle(new DepositUserBalanceCommand(dto.Amount, user.GetUserId()), cancellationToken));
    }

    private static async Task<Ok<IEnumerable<DepositResponseDto>>> GetMyDeposits(
        ClaimsPrincipal user,
        [FromServices]  GetUserDepositsQueryHandler queryHandler, 
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await queryHandler.Handle(new GetUserDepositsQuery(user.GetUserId()), cancellationToken));
    }
}