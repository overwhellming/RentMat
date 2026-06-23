namespace RentMat.Application.Exceptions.Devices;

public class DeviceIsNotAvailableException(int id) : Exception($"Device with id {id} is not available");