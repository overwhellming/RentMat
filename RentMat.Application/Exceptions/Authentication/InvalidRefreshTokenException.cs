using System.Net;

namespace RentMat.Application.Exceptions.Authentication;

public class InvalidRefreshTokenException() : Exception($"Refresh token is invalid, expired, or has been revoked"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    public string Title => "Invalid refresh token";
}