using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Handlers.Authentication;

namespace RentMat.API.Endpoints;

internal static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/login", Login)
            .WithName("UserLogin")
            .WithSummary("Authenticates a user and returns a JWT token")
            .Produces(200)
            .Produces(400)
            .Produces(401);

        group.MapPost("/register", Register)
            .WithName("UserRegistration")
            .WithSummary("Registers a user and returns a JWT token")
            .Produces(200)
            .Produces(400)
            .Produces(409);
    }

    private static async Task<IResult> Login(LoginDto dto, LoginHandler handler, CancellationToken cancellationToken)
    {
        var token = await handler.Handle(dto, cancellationToken);
        return Results.Ok(new AuthResponseDto(token));
    }

    private static async Task<IResult> Register(RegisterDto dto, RegisterHandler handler,
        CancellationToken cancellationToken)
    {
        var token = await handler.Handle(dto, cancellationToken);
        return Results.Ok(new AuthResponseDto(token));
    }
}