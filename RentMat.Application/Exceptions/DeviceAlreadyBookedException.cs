namespace RentMat.Application.Exceptions;

public class DeviceAlreadyBookedException(int id) : Exception($"Device with id {id} is already booked");