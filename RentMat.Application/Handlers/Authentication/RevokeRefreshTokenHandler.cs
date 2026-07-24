using Microsoft.EntityFrameworkCore;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Authentication;

public class RevokeRefreshTokenHandler
{
    private readonly AppDbContext _db;

    public RevokeRefreshTokenHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(int userId, CancellationToken cancellationToken)
    {
        var tokenEntry =
            await _db.RefreshTokenEntries.FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
        
        if (tokenEntry is null)
            throw new ActiveRefreshTokenNotFoundException(userId);

        tokenEntry.IsRevoked = true;
        await _db.SaveChangesAsync(cancellationToken);
    }
}