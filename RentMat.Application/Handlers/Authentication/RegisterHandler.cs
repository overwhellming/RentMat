using Microsoft.EntityFrameworkCore;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Services.Interfaces;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Authentication;

public class RegisterHandler
{
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterHandler(AppDbContext db, IFusionCache cache, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _cache = cache;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<string> Handle(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var formattedLogin = dto.Login.Trim();
        var formattedEmail = dto.Email.Trim().ToLowerInvariant();
        
        var userExists = await _db.Users.AnyAsync(u => u.Login == formattedLogin 
                                                       || u.Email == formattedEmail,
            cancellationToken);

        if (userExists)
            throw new UserAlreadyExistsException();

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12);

        var user = new User
        {
            Login = dto.Login.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            HashedPassword = hashedPassword,
            Balance = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByTagAsync("users");

        return _jwtTokenService.CreateToken(user);
    }
}