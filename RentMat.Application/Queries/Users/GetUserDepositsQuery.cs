using MediatR;
using RentMat.Application.DTOs.User;

namespace RentMat.Application.Queries.Users;

public record GetUserDepositsQuery(int UserId) : IRequest<IEnumerable<DepositResponseDto>>;