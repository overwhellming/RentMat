using Microsoft.EntityFrameworkCore;
using RentMat.Application.Exceptions;
using RentMat.Application.Exceptions.Users;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Users;

public class GetUserBalanceHandler
{
    private  readonly AppDbContext _db;

    public GetUserBalanceHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> Handle(int userId, CancellationToken cancellationToken)
    {
        var balance = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => (decimal?)u.Balance)
            .SingleOrDefaultAsync(cancellationToken);

        return balance ?? throw new UserNotFoundException(userId);
    }
}