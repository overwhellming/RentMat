using RentMat.Core.Enums;

namespace RentMat.Application.Queries.Users;

public record GetAllUsersQuery(int Page = 1, int PageSize = 10, string? Search = null, UserRole? Role = null);