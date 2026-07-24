using System.Net;

namespace RentMat.Application.Exceptions.Authentication;

public class ActiveRefreshTokenNotFoundException(int userId) : Exception($"Active refresh token for user id: {userId} was not found"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    public string Title => "Active refresh token not found";
}