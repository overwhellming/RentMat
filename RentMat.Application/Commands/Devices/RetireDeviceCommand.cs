using MediatR;

namespace RentMat.Application.Commands.Devices;

public record RetireDeviceCommand(int Id) : IRequest;