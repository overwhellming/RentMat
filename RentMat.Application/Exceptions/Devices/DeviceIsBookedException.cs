namespace RentMat.Application.Exceptions.Devices;

public class DeviceIsBookedException(int id) : Exception($"Device with id {id} is booked");