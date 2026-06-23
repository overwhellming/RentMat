namespace RentMat.Application.Exceptions.Booking;

public class BookingNotFoundException(int bookingId)
    : Exception($"Booking with id {bookingId} was not found");