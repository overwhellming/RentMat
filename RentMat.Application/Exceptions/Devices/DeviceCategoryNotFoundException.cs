namespace RentMat.Application.Exceptions.Devices;

public class DeviceCategoryNotFoundException(int id) : Exception($"Device category with id {id} was not found");