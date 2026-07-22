using System.Net;

namespace RentMat.Application.Exceptions.Booking;

public class ActiveBookingNotFoundException(int deviceId)
    : Exception($"Active booking for a device with id {deviceId} was not found"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public string Title => "Active booking for device was not found";
}
