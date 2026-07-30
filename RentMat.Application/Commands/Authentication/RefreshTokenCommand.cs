using MediatR;
using RentMat.Application.DTOs.Authentication;

namespace RentMat.Application.Commands.Authentication;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<TokenResponseDto>;