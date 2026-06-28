using System.Net;

namespace RentMat.Application.Exceptions.Booking;

public class BookingAccessDeniedException() : Exception("Access to booking is denied"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
    public string Title => "Internal server ";
}
