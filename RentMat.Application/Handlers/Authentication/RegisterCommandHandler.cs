using MediatR;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Commands.Authentication;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Exceptions.Users;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Authentication;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, TokenResponseDto>
{
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;

    public RegisterCommandHandler(AppDbContext db, IFusionCache cache, IMediator mediator)
    {
        _db = db;
        _cache = cache;
        _mediator = mediator;
    }

    public async Task<TokenResponseDto> Handle(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        var formattedLogin = command.Login.Trim();
        var formattedEmail = command.Email.Trim().ToLowerInvariant();

        var userExists = await _db.Users.AnyAsync(u => u.Login == formattedLogin
                                                       || u.Email == formattedEmail,
            cancellationToken);

        if (userExists)
            throw new UserAlreadyExistsException();

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(command.Password, 12);

        var user = new User
        {
            Login = command.Login.Trim(),
            Email = command.Email.Trim().ToLowerInvariant(),
            HashedPassword = hashedPassword,
            Balance = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByTagAsync("users");

        var tokenResponse = await _mediator.Send(new LoginCommand(command.Login, command.Password), cancellationToken);

        return tokenResponse;
    }
}