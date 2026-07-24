using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Services;
using RentMat.Application.Services.Interfaces;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Authentication;

public class RefreshTokenHandler
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _tokenService;
    
    public RefreshTokenHandler(AppDbContext db, IJwtTokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<TokenResponseDto> Handle(RefreshTokenDto dto, CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);

        if (principal is null)
            throw new InvalidAccessTokenException();

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new InvalidTokenClaimsException();

        var tokenEntry = await _db.RefreshTokenEntries
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Token == dto.RefreshToken && e.UserId.ToString() == userId, cancellationToken);

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