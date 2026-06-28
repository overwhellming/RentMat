using System.Net;

namespace RentMat.Application.Exceptions.Authentication;

public class InvalidCredentialsException() : Exception("Login credentials are invalid"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    public string Title => "Invalid credentials";
}