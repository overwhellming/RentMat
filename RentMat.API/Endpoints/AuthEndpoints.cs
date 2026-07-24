using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RentMat.API.Common.Security;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Handlers.Authentication;
using RentMat.Application.Handlers.Booking;

namespace RentMat.API.Endpoints;

internal static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .AllowAnonymous();

        group.MapPost("/login", Login)
            .WithName("UserLogin")
            .WithSummary("Authenticates a user and returns a JWT token")
            .ProducesProblem(400)
            .ProducesProblem(401);

        group.MapPost("/register", Register)
            .WithName("UserRegistration")
            .WithSummary("Registers a user and returns a JWT token")
            .ProducesValidationProblem()
            .ProducesProblem(400)
            .ProducesProblem(409);

        group.MapPost("/revoke", Revoke)
            .RequireAuthorization()
            .WithTags("TokenRevoking")
            .WithSummary("Revokes current user's refresh token")
            .ProducesProblem(401);
        
        group.MapPost("/refresh", Refresh)
            .WithTags("TokenRefreshing")
            .WithSummary("Refreshes current user's refresh token")
            .ProducesProblem(401);
    }

    private static async Task<Ok<TokenResponseDto>> Login(LoginDto dto, [FromServices] LoginHandler handler, CancellationToken cancellationToken)
    {
        var tokenResponse = await handler.Handle(dto, cancellationToken);
        return TypedResults.Ok(tokenResponse);
    }

    private static async Task<Ok<TokenResponseDto>> Register(RegisterDto dto,[FromServices]  RegisterHandler handler,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await handler.Handle(dto, cancellationToken);
        return TypedResults.Ok(tokenResponse);
    }
    
    private static async Task<Ok> Revoke(ClaimsPrincipal user, [FromServices] RevokeRefreshTokenHandler handler, 
        CancellationToken cancellationToken)
    {
        await handler.Handle(user.GetUserId(), cancellationToken);
        return TypedResults.Ok();
    }
    
    private static async Task<Ok<TokenResponseDto>> Refresh(RefreshTokenDto dto, [FromServices] RefreshTokenHandler handler, 
        CancellationToken cancellationToken)
    {
        var tokenResponse = await handler.Handle(dto, cancellationToken);
        return TypedResults.Ok(tokenResponse);
    }
}