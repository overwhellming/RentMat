using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.DTOs.User;
using RentMat.Application.Queries.Users;
using RentMat.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.Handlers.Users;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResponse<UserResponseDto>>
{
    public const int MaxPageSize = 50;
    public const int DefaultPageSize = 10;
    
    private readonly IFusionCache _cache;
    private readonly AppDbContext _db;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(AppDbContext db, IFusionCache cache, ILogger<GetAllUsersQueryHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResponse<UserResponseDto>> Handle(GetAllUsersQuery query,
        CancellationToken cancellationToken)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var role = query.Role;

        var cacheKey =
            $"users:page:{query.Page}:page-size:{query.PageSize}:search:{search ?? string.Empty}:role:{role?.ToString() ?? "all"}";

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
                var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

                var users = await usersQuery
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
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
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            },
            tags: [CacheTags.Users],
            token: cancellationToken
        );
    }
}