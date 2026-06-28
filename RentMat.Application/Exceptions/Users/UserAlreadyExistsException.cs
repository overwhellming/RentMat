using System.Net;

namespace RentMat.Application.Exceptions.Users;

public class UserAlreadyExistsException() : Exception("User with this login or email already exists"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    public string Title => "User already exists";
}