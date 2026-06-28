using System.Net;

namespace RentMat.Application.Exceptions.Devices;

public class DeviceIsBookedException(int id) : Exception($"Device with id {id} is booked"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    public string Title => "Device is already booked";
}
