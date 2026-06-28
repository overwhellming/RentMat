using System.Net;

namespace RentMat.Application.Exceptions.Devices;

public class DeviceCategoryNotFoundException(int id) : Exception($"Device category with id {id} was not found"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public string Title => "Device category was not found";
}