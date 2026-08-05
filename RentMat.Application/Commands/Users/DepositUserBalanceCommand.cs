using MediatR;
using RentMat.Application.DTOs.User;

namespace RentMat.Application.Commands.Users;

public record DepositUserBalanceCommand(decimal Amount, int UserId) : IRequest<DepositCreatedResponseDto>;