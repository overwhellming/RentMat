using MediatR;
using RentMat.Application.DTOs.Device;

namespace RentMat.Application.Commands.Devices;

public record CreateDeviceCommand(string Name, decimal HourRentPrice, int CategoryId) : IRequest<DeviceResponseDto>;