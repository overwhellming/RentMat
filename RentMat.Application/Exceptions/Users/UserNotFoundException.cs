using System.Net;

namespace RentMat.Application.Exceptions.Users;

public class UserNotFoundException(int id) : Exception($"User with id {id} was not found"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public string Title => "User was not found";
}
