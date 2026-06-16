namespace RentMat.Application.DTOs.Device;

public record DeviceResponseDto(int Id, string Name, decimal HourRentPrice, string CategoryName, string StatusName);