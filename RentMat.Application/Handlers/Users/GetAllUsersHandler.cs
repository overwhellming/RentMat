using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.User;
using RentMat.Core.Enums;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Users;

public class GetAllUsersHandler
{
    private const int MaxPageSize = 50;
    private const int DefaultPageSize = 10;

    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    private readonly ILogger<GetAllUsersHandler> _logger;

    public GetAllUsersHandler(AppDbContext db, IFusionCache cache, ILogger<GetAllUsersHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResponse<UserResponseDto>> Handle(int page, int pageSize, string? search, UserRole? role,
        CancellationToken cancellationToken)
    {
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = DefaultPageSize;
        else if (pageSize > MaxPageSize)
            pageSize = MaxPageSize;
        
        search = search?.Trim().ToLowerInvariant();

        var cacheKey =
            $"users:page:{page}:page-size:{pageSize}:search:{search ?? string.Empty}:role:{role?.ToString() ?? "all"}";

        return await _cache.GetOrSetAsync<PagedResponse<UserResponseDto>>(
            cacheKey,
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
                
                var query = _db.Users
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(u => EF.Functions.ILike(u.Login, $"%{search}%") ||
                                             EF.Functions.ILike(u.Email, $"%{search}%"));
                if (role != null)
                    query = query.Where(u => u.Role == role);

                var totalItems = await query.CountAsync(ct);
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var users = await query
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