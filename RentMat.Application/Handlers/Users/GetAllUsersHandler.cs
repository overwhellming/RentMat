using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.User;
using RentMat.Application.Queries;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Users;

public class GetAllUsersHandler
{
    public const int MaxPageSize = 50;
    public const int DefaultPageSize = 10;
    
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;
    private readonly ILogger<GetAllUsersHandler> _logger;

    public GetAllUsersHandler(AppDbContext db, IFusionCache cache, ILogger<GetAllUsersHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResponse<UserResponseDto>> Handle(GetAllUsersQuery query,
        CancellationToken cancellationToken)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1
            ? DefaultPageSize
            : Math.Min(query.PageSize, MaxPageSize);
        var search = query.Search?.Trim().ToLowerInvariant();
        var role = query.Role;

        var cacheKey =
            $"users:page:{page}:page-size:{pageSize}:search:{search ?? string.Empty}:role:{role?.ToString() ?? "all"}";

        return await _cache.GetOrSetAsync<PagedResponse<UserResponseDto>>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

                var usersQuery = _db.Users
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                    usersQuery = usersQuery.Where(u => EF.Functions.ILike(u.Login, $"%{search}%") ||
                                                       EF.Functions.ILike(u.Email, $"%{search}%"));
                if (role != null)
                    usersQuery = usersQuery.Where(u => u.Role == role);

                var totalItems = await usersQuery.CountAsync(ct);
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var users = await usersQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserResponseDto(
                        u.Id,
                        u.Login,
                        u.Email,
                        u.Role.ToString(),
                        u.Balance,
                        u.CreatedAt
                    ))
                    .ToListAsync(ct);

                return new PagedResponse<UserResponseDto>
                {
                    Items = users,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            },
            tags: [CacheTags.Users],
            token: cancellationToken
        );
    }
}