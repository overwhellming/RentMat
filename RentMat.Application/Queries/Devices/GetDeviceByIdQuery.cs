using MediatR;
using RentMat.Application.DTOs.Device;

namespace RentMat.Application.Queries.Devices;

public record GetDeviceByIdQuery(int Id) : IRequest<DeviceResponseDto>;