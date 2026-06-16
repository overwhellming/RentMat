namespace RentMat.Application.Booking.Exceptions;

public class DeviceNotFoundException (int id) : Exception($"Device with id {id} was not found");