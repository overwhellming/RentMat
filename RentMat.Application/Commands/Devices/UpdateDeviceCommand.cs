using MediatR;

namespace RentMat.Application.Commands.Devices;

public record UpdateDeviceCommand(int Id, string Name, decimal HourRentPrice, int CategoryId) : IRequest;