using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.User;
using RentMat.Application.Exceptions.Users;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Users;

public class GetUserByIdHandler
{
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    private readonly ILogger<GetUserByIdHandler> _logger;

    public GetUserByIdHandler(AppDbContext db, IFusionCache cache, ILogger<GetUserByIdHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UserResponseDto> Handle(int userId, CancellationToken cancellationToken)
    {
        var cacheKey = $"users:id:{userId}";

        return await _cache.GetOrSetAsync<UserResponseDto>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);

                var user = await _db.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => new UserResponseDto(
                        u.Id,
                        u.Login,
                        u.Email,
                        u.Role.ToString(),
                        u.Balance,
                        u.CreatedAt
                    ))
                    .SingleOrDefaultAsync(ct);

                return user ?? throw new UserNotFoundException(userId);
            },
            tags: [CacheTags.Users],
            token: cancellationToken
        );
    }
}