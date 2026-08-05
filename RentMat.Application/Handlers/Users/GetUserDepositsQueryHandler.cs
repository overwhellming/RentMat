using MediatR;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.DTOs.User;
using RentMat.Application.Queries.Users;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Users;

public class GetUserDepositsQueryHandler : IRequestHandler<GetUserDepositsQuery, IEnumerable<DepositResponseDto>>
{
    private readonly AppDbContext _db;

    public GetUserDepositsQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<DepositResponseDto>> Handle(GetUserDepositsQuery query,
        CancellationToken cancellationToken)
    {
        var deposits = await _db.Deposits
            .Where(d => d.UserId == query.UserId)
            .Select(d => new DepositResponseDto(
                d.Id,
                d.Amount,
                d.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return deposits;
    }
}