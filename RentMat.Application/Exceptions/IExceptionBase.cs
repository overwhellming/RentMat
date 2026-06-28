using System.Net;

namespace RentMat.Application.Exceptions;

public interface IExceptionBase
{
    HttpStatusCode StatusCode { get; }
    string Title { get; }
}