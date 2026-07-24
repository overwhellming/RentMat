using System.Net;

namespace RentMat.Application.Exceptions.Authentication;

public class InvalidTokenClaimsException() : Exception($"The token is missing required claims"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    public string Title => "Invalid token claims";
}