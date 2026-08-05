using MediatR;
using RentMat.Application.DTOs.Authentication;

namespace RentMat.Application.Commands.Authentication;

public record RegisterCommand(string Login, string Email, string Password) : IRequest<TokenResponseDto>;