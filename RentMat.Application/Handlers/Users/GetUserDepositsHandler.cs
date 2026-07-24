using Microsoft.EntityFrameworkCore;
using RentMat.Application.DTOs.User;
using RentMat.Application.Exceptions.Users;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Users;

public class GetUserDepositsHandler
{
    private readonly AppDbContext _db;

    public GetUserDepositsHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<DepositResponseDto>> Handle(int userId, CancellationToken cancellationToken)
    {
        var deposits = await _db.Deposits
            .Where(d => d.UserId == userId)
            .Select(d => new DepositResponseDto(
                d.Id,
                d.Amount,
                d.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return deposits;
    }
}