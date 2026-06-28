using System.Net;

namespace RentMat.Application.Exceptions.Users;

public class NotEnoughMoneyException (int id) : Exception($"User with id {id} doesn't have enough money"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    public string Title => "User doesn't have enough money";
}