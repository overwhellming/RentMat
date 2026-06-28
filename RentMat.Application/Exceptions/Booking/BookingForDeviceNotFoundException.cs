using System.Net;

namespace RentMat.Application.Exceptions.Booking;

public class BookingForDeviceNotFoundException(int deviceId)
    : Exception($"Booking for a device with id {deviceId} was not found"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public string Title => "Booking for device was not found";
}
