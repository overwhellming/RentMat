using Microsoft.EntityFrameworkCore;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Exceptions;
using RentMat.Application.Exceptions.Users;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Authentication;

public class RegisterHandler
{
    private readonly AppDbContext _db;

    public RegisterHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Login == dto.Login || u.Email == dto.Email,
            cancellationToken);

        if (userExists)
            throw new UserAlreadyExistsException();

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

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
    }
}