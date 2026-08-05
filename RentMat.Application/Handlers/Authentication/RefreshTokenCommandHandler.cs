using System.Security.Claims;
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

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponseDto>
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _tokenService;

    public RefreshTokenCommandHandler(AppDbContext db, IJwtTokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<TokenResponseDto> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(command.AccessToken);

        if (principal is null)
            throw new InvalidAccessTokenException();

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new InvalidTokenClaimsException();

        var tokenEntry = await _db.RefreshTokenEntries
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Token == command.RefreshToken && e.UserId.ToString() == userId,
                cancellationToken);

        if (tokenEntry is null || tokenEntry.IsRevoked || tokenEntry.ExpiresAt < DateTimeOffset.UtcNow)
            throw new InvalidRefreshTokenException();

        tokenEntry.IsRevoked = true;

        var newAccessToken = _tokenService.GenerateAccessToken(tokenEntry.User, out var expires);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        _db.RefreshTokenEntries.Add(new RefreshTokenEntry
        {
            Token = newRefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(JwtTokenService.RefreshTokenDays),
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = int.Parse(userId),
            IsRevoked = false
        });

        await _db.SaveChangesAsync(cancellationToken);
        return new TokenResponseDto(newAccessToken, newRefreshToken, expires);
    }
}