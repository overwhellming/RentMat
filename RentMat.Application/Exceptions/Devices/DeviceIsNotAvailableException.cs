using System.Net;

namespace RentMat.Application.Exceptions.Devices;

public class DeviceIsNotAvailableException(int id) : Exception($"Device with id {id} is not available"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    public string Title => "Device is not available";
}