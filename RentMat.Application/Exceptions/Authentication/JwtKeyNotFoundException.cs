using System.Net;

namespace RentMat.Application.Exceptions.Authentication;

public class JwtKeyNotFoundException() : Exception("JWT key was not found in configuration"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;
    public string Title => "Internal server error";
}