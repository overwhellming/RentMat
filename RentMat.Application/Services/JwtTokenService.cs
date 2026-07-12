using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }


    public string CreateToken(User user)
    {
        var jwt = _config.GetSection("Jwt");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var keyString = jwt["Key"] ?? throw new JwtKeyNotFoundException();
        var issuer = jwt["Issuer"] ?? throw new JwtKeyNotFoundException();
        var audience = jwt["Audience"] ?? throw new JwtKeyNotFoundException();

        if (!int.TryParse(jwt["ExpireMinutes"], out var expireMinutes)) expireMinutes = DefaultExpireMinutes;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}