using MediatR;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Commands.Authentication;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Application.Services;
using RentMat.Application.Services.Interfaces;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Authentication;

public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponseDto>
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(AppDbContext db, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<TokenResponseDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == command.Login, cancellationToken);

        if (user == null)
            throw new InvalidCredentialsException();

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(command.Password, user.HashedPassword);

        if (!isPasswordValid)
            throw new InvalidCredentialsException();

        var accessToken = _jwtTokenService.GenerateAccessToken(user, out var expires);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var refreshTokenEntry = new RefreshTokenEntry
        {
            Token = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(JwtTokenService.RefreshTokenDays),
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = user.Id,
            IsRevoked = false
        };
        _db.RefreshTokenEntries.Add(refreshTokenEntry);
        await _db.SaveChangesAsync(cancellationToken);

        return new TokenResponseDto(accessToken, refreshToken, expires);
    }
}