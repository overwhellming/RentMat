using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            .Produces(400)
            .Produces(401)
            .Produces(403);

        group.MapGet("/{id:int}", GetById)
            .RequireAuthorization(Policies.AdminOnly)
            .WithName("GetUserById")
            .WithSummary("Returns a user by id")
            .Produces(401)
            .Produces(403)
            .Produces(404);

        group.MapGet("/me", GetMe)
            .RequireAuthorization()
            .WithName("GetMe")
            .WithSummary("Returns the current user")
            .Produces(401);
    }

    private static async Task<Ok<PagedResponse<UserResponseDto>>> GetAll([AsParameters] GetAllUsersQuery query, GetAllUsersHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(query, cancellationToken));
    }

    private static async Task<Ok<UserResponseDto>> GetById(int id, GetUserByIdHandler handler, CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(id, cancellationToken));
    }

    private static async Task<Ok<UserResponseDto>> GetMe(ClaimsPrincipal user, GetUserByIdHandler handler,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await handler.Handle(user.GetUserId(), cancellationToken));
    }
}