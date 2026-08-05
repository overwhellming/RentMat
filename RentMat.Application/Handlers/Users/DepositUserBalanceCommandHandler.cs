using MediatR;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Commands.Users;
using RentMat.Application.Common;
using RentMat.Application.DTOs.User;
using RentMat.Application.Exceptions.Users;
using RentMat.Core.Models;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Users;

public class DepositUserBalanceCommandHandler : IRequestHandler<DepositUserBalanceCommand, DepositCreatedResponseDto>
{
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;

    public DepositUserBalanceCommandHandler(AppDbContext db, IFusionCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<DepositCreatedResponseDto> Handle(DepositUserBalanceCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user == null)
            throw new UserNotFoundException(command.UserId);

        var deposit = new Deposit
        {
            UserId = command.UserId,
            Amount = command.Amount,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Deposits.Add(deposit);
        user.Balance += command.Amount;
        await _db.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Users);

        return new DepositCreatedResponseDto(deposit.Id, deposit.Amount, deposit.CreatedAt, user.Balance);
    }
}