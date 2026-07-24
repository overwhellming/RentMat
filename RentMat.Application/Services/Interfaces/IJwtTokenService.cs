using System.Security.Claims;
using RentMat.Core.Models;

namespace RentMat.Application.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, out DateTimeOffset expires);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}