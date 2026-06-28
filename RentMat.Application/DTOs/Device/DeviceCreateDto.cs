namespace RentMat.Application.DTOs.Device;

public record DeviceCreateDto(string Name, decimal HourRentPrice, int CategoryId);