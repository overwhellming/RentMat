using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Handlers.Authentication;

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
    }

    private static async Task<Ok<AuthResponseDto>> Login( LoginDto dto, [FromServices] LoginHandler handler, CancellationToken cancellationToken)
    {
        var token = await handler.Handle(dto, cancellationToken);
        return TypedResults.Ok(new AuthResponseDto(token));
    }

    private static async Task<Ok<AuthResponseDto>> Register(RegisterDto dto,[FromServices]  RegisterHandler handler,
        CancellationToken cancellationToken)
    {
        var token = await handler.Handle(dto, cancellationToken);
        return TypedResults.Ok(new AuthResponseDto(token));
    }
}