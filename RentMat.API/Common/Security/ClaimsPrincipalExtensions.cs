using System.Security.Claims;

namespace RentMat.API.Common.Security;

internal static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value != null)
            return int.Parse(value);
        throw new UnauthorizedAccessException("Invalid user id claim");
    }
}