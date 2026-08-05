using MediatR;
using Microsoft.EntityFrameworkCore;
using RentMat.Application.Commands.Authentication;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Infrastructure.Data;

namespace RentMat.Application.Handlers.Authentication;

public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand>
{
    private readonly AppDbContext _db;

    public RevokeRefreshTokenCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(RevokeRefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var tokenEntry =
            await _db.RefreshTokenEntries.FirstOrDefaultAsync(e => e.UserId == command.UserId, cancellationToken);

        if (tokenEntry is null)
            throw new ActiveRefreshTokenNotFoundException(command.UserId);

        tokenEntry.IsRevoked = true;
        await _db.SaveChangesAsync(cancellationToken);
    }
}