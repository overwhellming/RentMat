using System.Security.Claims;

namespace RentMat.API.Common.Security;

internal static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var idValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !int.TryParse(idValue, out var userId)
            ? throw new UnauthorizedAccessException("Invalid user id claim")
            : userId;
    }
}