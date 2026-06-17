namespace RentMat.Application.Booking.Exceptions;

public class ActiveBookingNotFoundException(int deviceId)
    : Exception($"Active booking for a device with id {deviceId} was not found");