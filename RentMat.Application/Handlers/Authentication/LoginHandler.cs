using Microsoft.EntityFrameworkCore;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Application.Services.Interfaces;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Authentication;

public class LoginHandler
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginHandler(AppDbContext db, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<string> Handle(LoginDto dto, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == dto.Login, cancellationToken);

        if (user == null)
            throw new InvalidCredentialsException();

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword);

        if (!isPasswordValid)
            throw new InvalidCredentialsException();

        return _jwtTokenService.CreateToken(user);
    }
}