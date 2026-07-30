using MediatR;
using RentMat.Application.DTOs.Authentication;

namespace RentMat.Application.Commands.Authentication;

public record LoginCommand(string Login, string Password) : IRequest<TokenResponseDto>;