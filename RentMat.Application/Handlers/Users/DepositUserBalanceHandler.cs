using Microsoft.EntityFrameworkCore;
using RentMat.Application.DTOs.User;
using RentMat.Application.Exceptions.Users;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Users;

public class DepositUserBalanceHandler
{
    private readonly AppDbContext _db;

    public DepositUserBalanceHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DepositCreatedResponseDto> Handle(decimal amount, int userId, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new UserNotFoundException(userId);

        var deposit = new Deposit
        {
            UserId = userId,
            Amount = amount,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Deposits.Add(deposit);
        user.Balance += amount;
        await _db.SaveChangesAsync(cancellationToken);

        return new DepositCreatedResponseDto(deposit.Id, deposit.Amount, deposit.CreatedAt, user.Balance);
    }
}