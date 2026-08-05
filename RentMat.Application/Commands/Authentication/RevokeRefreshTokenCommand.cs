using MediatR;

namespace RentMat.Application.Commands.Authentication;

public record RevokeRefreshTokenCommand(int UserId) : IRequest;