using System.Net;

namespace RentMat.Application.Exceptions.Authentication;

public class InvalidAccessTokenException() : Exception($"The access token is invalid or expired"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    public string Title => "Invalid access token";
}