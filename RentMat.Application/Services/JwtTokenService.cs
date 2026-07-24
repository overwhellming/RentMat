using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Application.Services.Interfaces;
using RentMat.Core.Models;

namespace RentMat.Application.Services;

public class JwtTokenService : IJwtTokenService
{
    private const int DefaultExpireMinutes = 60;
    public const int RefreshTokenDays = 7;
    
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(User user, out DateTimeOffset expires)
    {
        var jwt = _config.GetSection("Jwt");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Login),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var keyString = jwt["Key"] ?? throw new JwtKeyNotFoundException();
        var issuer = jwt["Issuer"] ?? throw new JwtKeyNotFoundException();
        var audience = jwt["Audience"] ?? throw new JwtKeyNotFoundException();

        if (!int.TryParse(jwt["ExpireMinutes"], out var expireMinutes)) 
            expireMinutes = DefaultExpireMinutes;
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        expires = DateTimeOffset.UtcNow.AddMinutes(expireMinutes);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwt = _config.GetSection("Jwt");

        var keyString = jwt["Key"] ?? throw new JwtKeyNotFoundException();
        var issuer = jwt["Issuer"] ?? throw new JwtKeyNotFoundException();
        var audience = jwt["Audience"] ?? throw new JwtKeyNotFoundException();

        var tokenValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(keyString!))
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, tokenValidationParams, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}