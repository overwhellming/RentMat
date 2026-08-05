using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RentMat.API.Common.Security;
using RentMat.Application.Commands.Authentication;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Handlers.Authentication;
using RentMat.Application.Handlers.Booking;

namespace RentMat.API.Endpoints;

internal static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .WithName("UserLogin")
            .WithSummary("Authenticates a user and returns a JWT token")
            .ProducesValidationProblem()
            .ProducesProblem(401);

        group.MapPost("/register", Register)
            .AllowAnonymous()
            .WithName("UserRegistration")
            .WithSummary("Registers a user and returns a JWT token")
            .ProducesValidationProblem()
            .ProducesProblem(409);

        group.MapPost("/revoke", Revoke)
            .RequireAuthorization()
            .WithName("TokenRevoking")
            .WithSummary("Revokes current user's refresh token")
            .ProducesProblem(401);
        
        group.MapPost("/refresh", Refresh)
            .RequireAuthorization()
            .WithName("TokenRefreshing")
            .WithSummary("Refreshes current user's refresh token")
            .ProducesValidationProblem()
            .ProducesProblem(401);
    }

    private static async Task<Ok<TokenResponseDto>> Login(
        [FromBody] LoginDto dto, 
        [FromServices] IMediator mediator, 
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(dto.Login, dto.Password);
        var tokenResponse = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(tokenResponse);
    }

    private static async Task<Ok<TokenResponseDto>> Register(
        [FromBody] RegisterDto dto,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(dto.Login, dto.Email, dto.Password);
        var tokenResponse = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(tokenResponse);
    }
    
    private static async Task<Ok> Revoke(
        ClaimsPrincipal user, 
        [FromServices] IMediator mediator, 
        CancellationToken cancellationToken)
    {
        await mediator.Send(new RevokeRefreshTokenCommand(user.GetUserId()), cancellationToken);
        return TypedResults.Ok();
    }
    
    private static async Task<Ok<TokenResponseDto>> Refresh(
        [FromBody] RefreshTokenDto dto, 
        [FromServices] IMediator mediator, 
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(dto.AccessToken, dto.RefreshToken);
        var tokenResponse = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(tokenResponse);
    }
}