namespace RentMat.Application.DTOs.Device;

public sealed record DeviceResponseDto(int Id, string Name, decimal HourRentPrice, string CategoryName, string StatusName);