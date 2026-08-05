using MediatR;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Queries.Users;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Users;

public class GetUserBalanceQueryHandler : IRequestHandler<GetUserBalanceQuery, decimal>
{
    private readonly AppDbContext _db;

    public GetUserBalanceQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> Handle(GetUserBalanceQuery query, CancellationToken cancellationToken)
    {
        var balance = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == query.UserId)
            .Select(u => (decimal?)u.Balance)
            .SingleOrDefaultAsync(cancellationToken);

        return balance ?? throw new UserNotFoundException(query.UserId);
    }
}