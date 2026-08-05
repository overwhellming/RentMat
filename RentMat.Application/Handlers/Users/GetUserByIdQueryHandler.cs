using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.User;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Queries.Users;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Users;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponseDto>
{
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(AppDbContext db, IFusionCache cache, ILogger<GetUserByIdQueryHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UserResponseDto> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"users:id:{query.UserId}";

        return await _cache.GetOrSetAsync<UserResponseDto>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);

                var user = await _db.Users
                    .AsNoTracking()
                    .Where(u => u.Id == query.UserId)
                    .Select(u => new UserResponseDto(
                        u.Id,
                        u.Login,
                        u.Email,
                        u.Role.ToString(),
                        u.Balance,
                        u.CreatedAt
                    ))
                    .SingleOrDefaultAsync(ct);

                return user ?? throw new UserNotFoundException(query.UserId);
            },
            tags: [CacheTags.Users],
            token: cancellationToken
        );
    }
}