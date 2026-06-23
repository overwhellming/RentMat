namespace RentMat.Application.Exceptions.Booking;

public class BookingForDeviceNotFoundException(int deviceId)
    : Exception($"Booking for a device with id {deviceId} was not found");