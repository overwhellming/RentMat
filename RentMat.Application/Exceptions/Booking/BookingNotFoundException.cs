using System.Net;

namespace RentMat.Application.Exceptions.Booking;

public class BookingNotFoundException(int bookingId)
    : Exception($"Booking with id {bookingId} was not found"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public string Title => "Booking was not found";
}